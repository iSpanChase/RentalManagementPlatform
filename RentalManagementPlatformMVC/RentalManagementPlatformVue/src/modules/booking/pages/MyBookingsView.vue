<script setup>
import { ref, onMounted, computed } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';
import { useAuthStore } from '@/stores/auth';
import { formatDate } from '@/composables/useBookingFormatters';
import { usePagination } from '@/composables/usePagination';
import SimplePaginator from '@/components/SimplePaginator.vue';
import StatusBadge from '@/components/StatusBadge.vue';

const bookingStore = useBookingStore();
const router = useRouter();
const toast = useToast();
const auth = useAuthStore();

// 權限檢查方法
const canCancelBooking = auth.can('Booking.Delete');
const canCreateBooking = auth.can('Booking.Create');

const allBookings = ref([]);
const isLoading = ref(true);
const isError = ref(false);
const selectedBooking = ref(null);
const bookingToCancel = ref(null);
const isCancelling = ref(false);

// 篩選和排序狀態
const statusFilter = ref('');
const sortBy = ref('newest');
const searchKeyword = ref('');

// 篩選和排序後的訂單
const filteredAndSortedBookings = computed(() => {
  let filtered = [...allBookings.value];

  // 狀態篩選
  if (statusFilter.value) {
    filtered = filtered.filter((booking) => booking.paymentStatus === statusFilter.value);
  }

  // 關鍵字搜尋（房源名稱、訂單編號）
  if (searchKeyword.value.trim()) {
    const keyword = searchKeyword.value.toLowerCase().trim();
    filtered = filtered.filter(
      (booking) =>
        booking.room?.toLowerCase().includes(keyword) ||
        booking.orderNumber?.toLowerCase().includes(keyword)
    );
  }

  // 排序
  filtered.sort((a, b) => {
    switch (sortBy.value) {
      case 'newest':
        return new Date(b.createdAt) - new Date(a.createdAt);
      case 'oldest':
        return new Date(a.createdAt) - new Date(b.createdAt);
      case 'checkin-asc':
        return new Date(a.checkIn) - new Date(b.checkIn);
      case 'checkin-desc':
        return new Date(b.checkIn) - new Date(a.checkIn);
      case 'amount-high':
        return b.totalPrice - a.totalPrice;
      case 'amount-low':
        return a.totalPrice - b.totalPrice;
      default:
        return 0;
    }
  });

  return filtered;
});

// 使用篩選後的資料進行分頁
const {
  currentPage,
  totalPages,
  paginatedItems: paginatedBookings,
  onPageChange,
} = usePagination(filteredAndSortedBookings, 5);

/**
 * 立即付款
 */
const handlePayNow = async (orderNumber) => {
  isLoading.value = true;
  try {
    const result = await bookingStore.getDeferredPaymentForm(orderNumber);
    if (result.success && result.ecpayFormHtml) {
      const tempDiv = document.createElement('div');
      tempDiv.innerHTML = result.ecpayFormHtml;
      document.body.appendChild(tempDiv);
      const form = tempDiv.querySelector('form');
      if (form) {
        setTimeout(() => form.submit(), 500);
        toast.success('正在導向付款頁面...');
      } else {
        toast.error('無法載入付款表單，請聯繫客服');
      }
    } else {
      toast.error(result.message || '付款表單產生失敗');
    }
  } catch (error) {
    toast.error(error.message || '立即付款失敗');
  } finally {
    isLoading.value = false;
  }
};

/**
 * 查看預訂詳情
 */
const viewBookingDetails = (orderNumber, bookings) => {
  const booking = bookings.find((b) => b.orderNumber === orderNumber);
  if (booking) {
    selectedBooking.value = booking;
  }
};

/**
 * 聯繫房東
 */
const handleContactHost = () => {
  toast.info('聯繫房東功能開發中...');
};

/**
 * 重新預訂
 */
const handleRebook = (booking) => {
  // 嘗試多種可能的房源 ID 欄位名稱
  const roomId = booking.roomId || booking.RoomId || booking.room_id;

  if (!roomId) {
    console.log('Booking data:', booking); // 調試用
    toast.error('無法找到房源資訊，請聯繫客服');
    return;
  }

  try {
    // 導向到房源詳情頁面
    router.push({
      name: 'room-detail',
      params: { id: roomId.toString() },
    });

    toast.info('正在前往房源頁面，您可以重新預訂');
  } catch (error) {
    console.error('Navigation error:', error);
    toast.error('頁面跳轉失敗，請稍後再試');
  }
};

/**
 * 開啟取消確認 Modal
 */
const openCancelConfirmModal = (booking) => {
  bookingToCancel.value = booking;
};

/**
 * 確認取消預訂
 */
const confirmCancellation = async () => {
  if (!bookingToCancel.value) return;

  // 嘗試多種可能的預訂 ID 欄位名稱
  const bookingId =
    bookingToCancel.value.bookingId || bookingToCancel.value.BookingId || bookingToCancel.value.id;

  if (!bookingId) {
    toast.error('無法找到預訂資訊');
    return;
  }

  isCancelling.value = true;
  try {
    const result = await bookingStore.cancelBooking(bookingId);

    // 更新本地資料
    const targetBookingId =
      bookingToCancel.value.bookingId ||
      bookingToCancel.value.BookingId ||
      bookingToCancel.value.id;
    const index = allBookings.value.findIndex(
      (b) => (b.bookingId || b.BookingId || b.id) === targetBookingId
    );
    if (index !== -1) {
      allBookings.value[index].paymentStatus = 'cancelled';
    }

    toast.success('預訂已成功取消');
    bookingToCancel.value = null;
  } catch (error) {
    toast.error(error.message || '取消預訂失敗');
  } finally {
    isCancelling.value = false;
  }
};

/**
 * 載入我的預訂
 */
onMounted(async () => {
  // 檢查使用者是否已登入
  if (!auth.isAuthenticated.value) {
    toast.error('請先登入查看預訂記錄');
    router.push({ name: 'LoginView' });
    return;
  }

  isLoading.value = true;
  isError.value = false;

  try {
    const userId = auth.state.profile?.userId;
    if (!userId) {
      router.push({ name: 'LoginView' });
      return;
    }

    // 使用新的fetchMyBookings方法
    const fetchedBookings = await bookingStore.fetchMyBookings(userId);
    allBookings.value = fetchedBookings || [];

    if (allBookings.value.length === 0) {
      toast.info('目前沒有任何預訂記錄');
    }
  } catch (error) {
    console.error('載入預訂失敗:', error);
    toast.error(error.message || '載入預訂資料失敗');
    isError.value = true;
  } finally {
    isLoading.value = false;
  }
});

/**
 * 清除所有篩選條件
 */
const clearFilters = () => {
  statusFilter.value = '';
  searchKeyword.value = '';
  sortBy.value = 'newest';
};

/**
 * 重新載入資料
 */
const reloadData = async () => {
  isLoading.value = true;
  isError.value = false;

  try {
    const userId = auth.state.profile?.userId;
    if (!userId) {
      toast.error('無法獲取使用者資訊，請重新登入');
      router.push({ name: 'LoginView' });
      return;
    }

    const fetchedBookings = await bookingStore.fetchMyBookings(userId);
    allBookings.value = fetchedBookings || [];

    if (allBookings.value.length === 0) {
      toast.info('目前沒有任何預訂記錄');
    }
  } catch (error) {
    console.error('載入預訂失敗:', error);
    toast.error(error.message || '載入預訂資料失敗');
    isError.value = true;
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="my-bookings-page">
    <h1>我的預訂</h1>

    <!-- Loading -->
    <div v-if="(isLoading || bookingStore.isLoading) && !isCancelling" class="loading-overlay">
      <div class="loading-content">
        <div class="loading-spinner"></div>
        <p>{{ bookingStore.isLoading ? '正在處理您的預訂...' : '正在載入預訂資料...' }}</p>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="isError" class="error-message-container">
      <div class="error-card">
        <h2>無法載入頁面</h2>
        <p>抱歉，載入預訂資料時發生錯誤，請稍後再試。</p>
        <button @click="router.push({ name: 'home' })" class="btn-back-home">返回首頁</button>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="allBookings.length === 0" class="error-message-container">
      <div class="error-card">
        <h2>尚無預訂</h2>
        <p>您目前沒有任何預訂。</p>
        <button @click="router.push({ name: 'home' })" class="btn-back-home">開始探索房源</button>
      </div>
    </div>

    <!-- Content -->
    <div v-else class="container">
      <!-- 篩選和排序控制 -->
      <div class="filter-controls">
        <div class="filter-section">
          <div class="search-box">
            <input
              type="text"
              v-model="searchKeyword"
              placeholder="搜尋房源名稱或訂單編號..."
              class="search-input"
            />
            <i class="fa-solid fa-search search-icon"></i>
            <button v-if="searchKeyword" @click="searchKeyword = ''" class="clear-btn">
              <i class="fa-solid fa-times"></i>
            </button>
          </div>

          <div class="filter-dropdowns">
            <select v-model="statusFilter" class="filter-select">
              <option value="">所有狀態</option>
              <option value="pending">待付款</option>
              <option value="deferred">延後付款</option>
              <option value="completed">已付款</option>
              <option value="cancelled">已取消</option>
              <option value="refunded">已退款</option>
            </select>

            <select v-model="sortBy" class="sort-select">
              <option value="newest">最新訂單</option>
              <option value="oldest">最舊訂單</option>
              <option value="checkin-asc">入住日期（近到遠）</option>
              <option value="checkin-desc">入住日期（遠到近）</option>
              <option value="amount-high">金額（高到低）</option>
              <option value="amount-low">金額（低到高）</option>
            </select>
          </div>
        </div>

        <div class="results-info">
          <span class="results-count">
            找到 {{ filteredAndSortedBookings.length }} 筆訂單
            <span v-if="allBookings.length !== filteredAndSortedBookings.length">
              （共 {{ allBookings.length }} 筆）
            </span>
          </span>

          <button v-if="statusFilter || searchKeyword" @click="clearFilters" class="clear-all-btn">
            <i class="fa-solid fa-filter-circle-xmark"></i>
            清除篩選
          </button>
        </div>
      </div>

      <div class="bookings-list">
        <div v-for="booking in paginatedBookings" :key="booking.orderNumber" class="booking-card">
          <div class="card-image-wrapper">
            <img
              :src="booking.roomImageUrl || 'https://placehold.co/220x180/EBEBEB/717171?text=Room'"
              alt="房源圖片"
              class="room-image"
            />
          </div>

          <div class="card-details-wrapper">
            <div class="card-section top-section">
              <div class="room-info">
                <span class="room-location">{{ booking.billingCountry || '城市, 國家' }}</span>
                <h3>{{ booking.room }}</h3>
              </div>
              <StatusBadge :status="booking.paymentStatus" />
            </div>

            <div class="card-section mid-section">
              <div class="info-item">
                <i class="fa-solid fa-calendar-days"></i>
                <span>{{ formatDate(booking.checkIn) }} - {{ formatDate(booking.checkOut) }}</span>
              </div>
              <div class="info-item">
                <i class="fa-solid fa-user-group"></i>
                <span>{{ booking.guestCount }} 位住客</span>
              </div>
              <div class="info-item order-number">
                <i class="fa-solid fa-hashtag"></i>
                <span>訂單: {{ booking.orderNumber }}</span>
              </div>
            </div>

            <div class="card-section bottom-section">
              <div class="total-price-area">
                <span>總金額</span>
                <p class="total-price-value">
                  <strong>${{ booking.totalPrice.toLocaleString() }} TWD</strong>
                </p>
              </div>
              <div class="card-actions">
                <button
                  v-if="booking.paymentStatus === 'deferred'"
                  @click="handlePayNow(booking.orderNumber)"
                  class="btn-pay-now"
                  :disabled="isLoading"
                >
                  立即付款
                </button>
                <button
                  class="btn-contact"
                  @click="handleContactHost"
                  v-if="
                    booking.paymentStatus !== 'cancelled' && booking.paymentStatus !== 'refunded'
                  "
                >
                  聯繫房東
                </button>
                <button
                  class="btn-cancel"
                  data-bs-toggle="modal"
                  data-bs-target="#cancelConfirmModal"
                  @click="openCancelConfirmModal(booking)"
                  v-if="
                    canCancelBooking &&
                    booking.paymentStatus !== 'cancelled' &&
                    booking.paymentStatus !== 'refunded'
                  "
                  :disabled="isCancelling || isLoading"
                >
                  取消預訂
                </button>
                <button
                  class="btn-rebook"
                  @click="handleRebook(booking)"
                  v-if="
                    canCreateBooking &&
                    ['cancelled', 'completed', 'refunded'].includes(booking.paymentStatus)
                  "
                  :disabled="isLoading || isCancelling"
                >
                  重新預訂
                </button>
                <button
                  class="btn-details"
                  data-bs-toggle="modal"
                  data-bs-target="#orderDetailModal"
                  @click="viewBookingDetails(booking.orderNumber, allBookings)"
                >
                  查看詳情
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <SimplePaginator
        :current-page="currentPage"
        :total-pages="totalPages"
        @page-changed="onPageChange"
      />
    </div>
  </div>

  <Teleport to="body">
    <!-- 訂單詳情 Modal -->
    <div
      class="modal fade"
      id="orderDetailModal"
      tabindex="-1"
      aria-labelledby="orderDetailModalLabel"
      aria-hidden="true"
    >
      <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content" v-if="selectedBooking">
          <div class="modal-header">
            <h5 class="modal-title" id="orderDetailModalLabel">
              訂單詳情 #{{ selectedBooking.orderNumber }}
            </h5>
            <button
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="Close"
            ></button>
          </div>
          <div class="modal-body">
            <div class="detail-section">
              <h6><i class="fa-solid fa-house"></i> 預訂房源</h6>
              <div class="detail-row">
                <p><strong>房源名稱:</strong></p>
                <p>{{ selectedBooking.room }}</p>
              </div>
              <div class="detail-row">
                <p><strong>入住/退房:</strong></p>
                <p>
                  {{ formatDate(selectedBooking.checkIn) }} -
                  {{ formatDate(selectedBooking.checkOut) }} ({{ selectedBooking.guestCount }}人)
                </p>
              </div>
              <div class="detail-row">
                <p><strong>訂單建立:</strong></p>
                <p>{{ formatDate(selectedBooking.createdAt) }}</p>
              </div>
              <div class="detail-row">
                <p><strong>目前狀態:</strong></p>
                <p>
                  <StatusBadge :status="selectedBooking.paymentStatus" />
                </p>
              </div>
            </div>
            <hr />
            <div class="detail-section">
              <h6><i class="fa-solid fa-credit-card"></i> 價格與付款</h6>
              <div class="detail-row">
                <p><strong>總金額:</strong></p>
                <p class="price-value">TWD {{ selectedBooking.totalPrice.toLocaleString() }}</p>
              </div>
            </div>
            <hr />
            <div class="detail-section">
              <h6><i class="fa-solid fa-user"></i> 聯絡人資訊</h6>
              <div class="detail-row">
                <p><strong>姓名:</strong></p>
                <p>{{ selectedBooking.contactName }}</p>
              </div>
              <div class="detail-row">
                <p><strong>Email:</strong></p>
                <p>{{ selectedBooking.contactEmail }}</p>
              </div>
              <div class="detail-row">
                <p><strong>電話:</strong></p>
                <p>{{ selectedBooking.contactPhone }}</p>
              </div>
              <div class="detail-row" v-if="selectedBooking.contactNotes">
                <p><strong>特殊需求:</strong></p>
                <p class="notes-text">{{ selectedBooking.contactNotes }}</p>
              </div>
            </div>
            <hr />
            <div class="detail-section">
              <h6><i class="fa-solid fa-address-book"></i> 帳單地址</h6>
              <div class="address-block">
                {{ selectedBooking.billingCountry }}<br />
                {{ selectedBooking.billingZipCode }} {{ selectedBooking.billingCity }}<br />
                {{ selectedBooking.billingState }} {{ selectedBooking.billingStreet }}<br />
                {{ selectedBooking.billingApartment }}
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">關閉</button>
          </div>
        </div>
      </div>
    </div>

    <!-- 取消確認 Modal -->
    <div
      class="modal fade"
      id="cancelConfirmModal"
      tabindex="-1"
      aria-labelledby="cancelConfirmModalLabel"
      aria-hidden="true"
    >
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="cancelConfirmModalLabel">確認取消預訂</h5>
            <button
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="Close"
            ></button>
          </div>
          <div class="modal-body">
            您確定要取消這筆訂單 (編號: {{ bookingToCancel?.orderNumber }}) 嗎？
            <br />
            <small class="text-muted">請注意，取消政策可能適用。</small>
          </div>
          <div class="modal-footer">
            <button
              type="button"
              class="btn btn-secondary"
              data-bs-dismiss="modal"
              :disabled="isCancelling"
            >
              關閉
            </button>
            <button
              type="button"
              class="btn btn-danger"
              @click="confirmCancellation"
              :disabled="isCancelling"
              data-bs-dismiss="modal"
            >
              <span
                v-if="isCancelling"
                class="spinner-border spinner-border-sm"
                role="status"
              ></span>
              <span v-else>確認取消</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" scoped>
@use "sass:color";

$primary-color: #222;
$secondary-color: #008489;
$danger-color: #d9534f;
$border-color: #ebebeb;
$background-light: #f9f9f9;
$text-light: #717171;
$text-dark: #484848;

.my-bookings-page {
  max-width: 1024px;
  margin: 0 auto;
  padding: 20px;
  min-height: 100vh;

  h1 {
    font-size: 28px;
    font-weight: bold;
    margin-left: 10px;
    margin-bottom: 20px;
    color: #222;
  }
}

.container {
  width: 100%;
  max-width: 900px;
  margin: 0 auto;
}

/* Loading & Error Styles */
.loading-overlay,
.error-message-container {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(248, 249, 250, 0.8);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
  padding: 20px;
}

.loading-content,
.error-card {
  background: white;
  padding: 40px;
  border-radius: 16px;
  text-align: center;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  max-width: 350px;
  width: 90%;

  p {
    margin-top: 16px;
    color: #333;
    font-size: 16px;
    font-weight: 500;
  }
}

.loading-spinner {
  width: 50px;
  height: 50px;
  border: 4px solid #f0f0f0;
  border-top: 4px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;
}

.error-card {
  h2 {
    font-size: 22px;
    font-weight: 600;
    color: #d9534f;
    margin-bottom: 15px;
  }
  p {
    color: #484848;
    line-height: 1.6;
  }
  .btn-back-home {
    margin-top: 20px;
    padding: 10px 20px;
    background-color: #007bff;
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: background-color 0.2s;
    &:hover {
      background-color: #0056b3;
    }
  }
}

.bookings-list {
  display: grid;
  gap: 24px;

  .booking-card {
    display: flex;
    background: white;
    border: 1px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
    overflow: hidden;
    transition: box-shadow 0.3s ease;

    &:hover {
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
    }

    .card-image-wrapper {
      width: 220px;
      flex-shrink: 0;

      .room-image {
        width: 100%;
        height: 100%;
        object-fit: cover;
      }
    }

    .card-details-wrapper {
      flex: 1;
      display: flex;
      flex-direction: column;
      padding: 20px 24px;
      gap: 12px;
    }

    .card-section {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
    }

    .top-section {
      .room-info {
        .room-location {
          display: block;
          font-size: 14px;
          color: $text-light;
        }
        h3 {
          margin: 2px 0 0;
          font-size: 20px;
          font-weight: 600;
          color: $primary-color;
        }
      }
    }

    .mid-section {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      gap: 8px;
      padding: 12px 0;
      border-top: 1px solid $border-color;
      flex-grow: 1;
      justify-content: center;

      .info-item {
        display: flex;
        align-items: center;
        color: $text-dark;
        font-size: 14px;

        i {
          margin-right: 10px;
          color: $text-light;
          width: 16px;
          text-align: center;
        }

        &.order-number {
          color: $text-light;
          font-size: 13px;
        }
      }
    }

    .bottom-section {
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
      border-top: 1px solid $border-color;
      padding-top: 16px;

      .total-price-area {
        text-align: left;
        span {
          font-size: 13px;
          color: $text-light;
        }
        .total-price-value {
          margin: 0;
          strong {
            font-size: 20px;
            font-weight: 700;
            color: $primary-color;
          }
        }
      }

      .card-actions {
        display: flex;
        flex-wrap: wrap;
        justify-content: flex-end;
        gap: 8px;
      }
    }
  }
}

%btn-base {
  padding: 8px 14px;
  border-radius: 8px;
  border: 1px solid;
  font-weight: 600;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
}

.btn-pay-now,
.btn-details,
.btn-contact,
.btn-cancel,
.btn-rebook {
  @extend %btn-base;
}

.btn-details {
  background-color: $background-light;
  color: $primary-color;
  border-color: #ddd;

  &:hover {
    background-color: $primary-color;
    color: white;
    border-color: $primary-color;
  }
}

.btn-rebook {
  background-color: $secondary-color;
  color: white;
  border-color: $secondary-color;

  &:hover {
    background-color: color.adjust($secondary-color, $lightness: -10%);
    border-color: color.adjust($secondary-color, $lightness: -10%);
  }
}

.btn-pay-now {
  background-color: $secondary-color;
  color: white;
  border-color: $secondary-color;
  box-shadow: 0 2px 8px rgba(0, 132, 137, 0.3);

  &:hover {
    background-color: color.adjust($secondary-color, $lightness: -10%);
    border-color: color.adjust($secondary-color, $lightness: -10%);
    box-shadow: none;
  }

  &:disabled {
    background-color: #ccc;
    border-color: #ccc;
    color: $text-light;
    cursor: not-allowed;
    box-shadow: none;
  }
}

.btn-contact {
  background-color: white;
  color: $secondary-color;
  border-color: $secondary-color;

  &:hover {
    background-color: $secondary-color;
    color: white;
  }
}

.btn-cancel {
  background-color: white;
  color: $danger-color;
  border-color: $danger-color;

  &:hover {
    background-color: $danger-color;
    color: white;
  }

  &:disabled {
    background-color: #ccc;
    border-color: #ccc;
    color: $text-light;
    cursor: not-allowed;
  }
}

.order-status {
  padding: 5px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  display: inline-block;
  white-space: nowrap;
}

.status-deferred,
.status-unpaid {
  background-color: #fff3cd;
  color: #856404;
}
.status-completed,
.status-paid {
  background-color: #d4edda;
  color: #155724;
}
.status-cancelled {
  background-color: #f8d7da;
  color: #721c24;
}
.status-refunded {
  background-color: #e2e3e5;
  color: #383d41;
}

.modal-content {
  border-radius: 15px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);

  .modal-header {
    border-bottom: 1px solid $border-color;
    padding: 20px 25px;
    .modal-title {
      font-weight: 600;
      color: $primary-color;
    }
  }

  .modal-body {
    padding: 25px;
  }
}

.detail-section {
  margin-bottom: 25px;
  h6 {
    font-size: 16px;
    font-weight: 700;
    margin-bottom: 15px;
    color: $primary-color;
    padding-bottom: 5px;
    border-bottom: 1px dashed #f0f0f0;
    i {
      margin-right: 8px;
      color: $secondary-color;
    }
  }
}

.detail-row {
  display: grid;
  grid-template-columns: 120px 1fr;
  margin-bottom: 10px;
  font-size: 14px;
  align-items: start;

  p {
    margin: 0;
    line-height: 1.6;
  }
  strong {
    color: $text-light;
    font-weight: 500;
  }
  .price-value {
    font-weight: 700;
    color: $secondary-color;
    font-size: 16px;
  }
}

.notes-text {
  background-color: #f7f7f7;
  padding: 10px;
  border-radius: 8px;
  color: $primary-color;
  white-space: pre-wrap;
  font-style: italic;
}

.address-block {
  background-color: #f7f7f7;
  padding: 15px;
  border-radius: 8px;
  line-height: 1.8;
  color: $primary-color;
  font-size: 14px;
}

hr {
  border-color: $border-color;
  margin: 20px 0;
}

.modal-footer {
  border-top: 1px solid $border-color;
  padding: 15px 25px;
}

.btn-secondary {
  background-color: $text-light;
  border-color: $text-light;
  color: white;
  padding: 8px 20px;
  border-radius: 8px;

  &:hover {
    background-color: color.adjust($text-light, $lightness: -10%);
    border-color: color.adjust($text-light, $lightness: -10%);
  }
}

@media (max-width: 768px) {
  .container {
    padding: 0 15px;
  }
  .my-bookings-page {
    padding: 20px 0;
  }
  h1 {
    font-size: 24px;
    margin-bottom: 20px;
  }

  .bookings-list .booking-card {
    flex-direction: column;
    .card-image-wrapper {
      width: 100%;
      height: 200px;
    }
    .card-details-wrapper {
      padding: 16px;
      gap: 16px;
    }
    .bottom-section {
      flex-direction: column;
      align-items: stretch;
      gap: 12px;
      padding-top: 12px;
      .total-price-area {
        text-align: left;
      }
      .card-actions {
        width: 100%;
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 10px;
        .btn-pay-now {
          grid-column: 1 / -1;
        }
      }
    }
  }

  .modal-body {
    padding: 15px;
  }
  .detail-row {
    grid-template-columns: 90px 1fr;
  }
}

.btn-danger {
  background-color: $danger-color;
  border-color: $danger-color;
  color: white;

  &:hover {
    background-color: color.adjust($danger-color, $lightness: -10%);
    border-color: color.adjust($danger-color, $lightness: -10%);
  }
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

// 篩選控制樣式
.filter-controls {
  background: white;
  border: 1px solid $border-color;
  border-radius: 16px;
  padding: 20px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);

  .filter-section {
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin-bottom: 16px;

    @media (min-width: 768px) {
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
    }
  }

  .search-box {
    position: relative;
    flex: 1;
    max-width: 400px;

    .search-icon {
      position: absolute;
      left: 12px;
      top: 50%;
      transform: translateY(-50%);
      color: $text-light;
      font-size: 14px;
      z-index: 1;
      pointer-events: none;
    }

    .search-input {
      width: 100%;
      padding: 12px 16px 12px 40px;
      border: 1px solid $border-color;
      border-radius: 8px;
      font-size: 14px;
      transition: border-color 0.2s;

      &:focus {
        outline: none;
        border-color: $secondary-color;
        box-shadow: 0 0 0 3px rgba(0, 132, 137, 0.1);
      }

      &::placeholder {
        color: $text-light;
      }
    }

    .clear-btn {
      position: absolute;
      right: 8px;
      top: 50%;
      transform: translateY(-50%);
      background: none;
      border: none;
      color: $text-light;
      cursor: pointer;
      padding: 4px;
      border-radius: 4px;
      font-size: 12px;
      z-index: 1;

      &:hover {
        background: #f0f0f0;
        color: $text-dark;
      }
    }
  }

  .filter-dropdowns {
    display: flex;
    gap: 12px;
    flex-wrap: wrap;

    .filter-select,
    .sort-select {
      padding: 10px 12px;
      border: 1px solid $border-color;
      border-radius: 8px;
      background: white;
      font-size: 14px;
      color: $text-dark;
      cursor: pointer;
      transition: border-color 0.2s;
      min-width: 140px;

      &:focus {
        outline: none;
        border-color: $secondary-color;
      }

      &:hover {
        border-color: #bbb;
      }
    }
  }

  .results-info {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding-top: 16px;
    border-top: 1px solid $border-color;
    flex-wrap: wrap;
    gap: 12px;

    .results-count {
      font-size: 14px;
      color: $text-light;

      span {
        color: $text-light;
        font-weight: normal;
      }
    }

    .clear-all-btn {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 6px 12px;
      background: #fff3cd;
      border: 1px solid #ffc107;
      border-radius: 6px;
      color: #856404;
      font-size: 13px;
      cursor: pointer;
      transition: all 0.2s;

      &:hover {
        background: #ffc107;
        color: white;
      }

      i {
        font-size: 14px;
      }
    }
  }
}

// 手機版響應式調整
@media (max-width: 768px) {
  .filter-controls {
    padding: 16px;

    .filter-dropdowns {
      .filter-select,
      .sort-select {
        flex: 1;
        min-width: auto;
      }
    }

    .results-info {
      flex-direction: column;
      align-items: flex-start;
      gap: 8px;
    }
  }
}
</style>
