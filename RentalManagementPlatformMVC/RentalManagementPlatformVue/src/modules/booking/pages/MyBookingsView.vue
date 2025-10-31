<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useRoute, useRouter } from 'vue-router';
import { Modal } from 'bootstrap';
import { useToast } from 'vue-toastification';
import SimplePaginator from '@/components/SimplePaginator.vue';

const toast = useToast();
const bookingStore = useBookingStore();
const router = useRouter();
const route = useRoute();

const allBookings = ref([]);
const isLoading = ref(false);
const selectedBooking = ref(null);
const cancelModal = ref(null);
const bookingToCancel = ref(null);
const currentPage = ref(1);
const itemsPerPage = 5;

const totalPages = computed(() => Math.ceil(allBookings.value.length / itemsPerPage));
const paginatedBookings = computed(() => {
  const startIndex = (currentPage.value - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  return allBookings.value.slice(startIndex, endIndex);
});

function onPageChange(page) {
  currentPage.value = page;
}

/**
 * 格式化日期
 */
const formatDate = (dateStr) => {
  const date = new Date(dateStr);
  return date.toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};

/**
 * 取得狀態文字
 */
const getStatusText = (status) => {
  switch (status) {
    case 'deferred':
    case 'unpaid':
      return '待付款';
    case 'completed':
    case 'paid':
      return '已完成';
    case 'cancelled':
      return '已取消';
    case 'refunded':
      return '已退款';
    default:
      return status || '未知狀態';
  }
};

/**
 * 查看詳情
 */
const viewDetails = (orderNumber) => {
  const booking = allBookings.value.find(b => b.orderNumber === orderNumber);
  if (!booking) return;
  selectedBooking.value = booking;
};

const handleModalHidden = () => {
  selectedBooking.value = null;
};

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
      toast.error(result.message || '獲取付款表單失敗');
    }
  } catch (error) {
    toast.error(error.message || '付款失敗，請稍後再試');
  } finally {
    isLoading.value = false;
  }
};

/**
 * 聯繫房東
 */
const handleContactHost = () => {
  toast.info('聯繫房東功能開發中');
};

/**
 * 開啟取消確認
 */
const openCancelConfirmModal = (booking) => {
  bookingToCancel.value = booking;
  cancelModal.value?.show();
};

/**
 * 確認取消
 */
const confirmCancellation = async () => {
  if (!bookingToCancel.value) return;

  const bookingId = bookingToCancel.value.bookingId;
  isLoading.value = true;

  try {
    const response = await bookingStore.cancelBooking(bookingId);
    if (response?.success && response.booking) {
      toast.success(response.message || '訂單已成功取消');
      allBookings.value = allBookings.value.map(b =>
        b.bookingId === response.booking.bookingId ? response.booking : b
      );
    } else {
      toast.error(response?.message || '取消失敗');
    }
  } catch (error) {
    const msg = error.response?.data?.message || error.message || '取消時發生錯誤';
    toast.error(msg);
  } finally {
    isLoading.value = false;
    cancelModal.value?.hide();
    bookingToCancel.value = null;
  }
};

/**
 * 重新預訂
 */
const handleRebook = () => {
  router.push({ name: 'BookingConfirmView' });
};

onMounted(async () => {
  isLoading.value = true;

  const modalEl = document.getElementById('orderDetailModal');
  if (modalEl) {
    modalEl.addEventListener('hidden.bs.modal', handleModalHidden);
  }

  const cancelEl = document.getElementById('cancelConfirmModal');
  if (cancelEl) {
    cancelModal.value = new Modal(cancelEl);
  }

  try {
    let fetchedBookings = [];

    if (route.query.orderNumber) {
      const single = await bookingStore.fetchBookingByOrderNumber(route.query.orderNumber);
      if (single) fetchedBookings.push(single);
    } else {
      const userId = route.query.userId || 1;
      fetchedBookings = await bookingStore.fetchUserBookings(userId);
    }

    allBookings.value = fetchedBookings.map(b => ({ ...b }));
  } catch (error) {
    toast.error('無法載入訂單資料');
  } finally {
    isLoading.value = false;
  }
});

onUnmounted(() => {
  const modalEl = document.getElementById('orderDetailModal');
  if (modalEl) {
    modalEl.removeEventListener('hidden.bs.modal', handleModalHidden);
  }
});
</script>

<template>
  <div class="my-bookings-page">
    <div class="container">
      <h1>我的預訂</h1>

      <div v-if="isLoading" class="loading-spinner">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">載入中...</span>
        </div>
      </div>

      <div v-else-if="allBookings.length === 0" class="no-bookings">
        <p>您目前沒有任何預訂。</p>
        <router-link to="/">
          <button class="btn-primary">開始探索房源</button>
        </router-link>
      </div>

      <div v-else>
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
                <span :class="['order-status', `status-${booking.paymentStatus}`]">
                  {{ getStatusText(booking.paymentStatus) }}
                </span>
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
                    v-if="booking.paymentStatus !== 'cancelled' && booking.paymentStatus !== 'refunded'"
                  >
                    聯繫房東
                  </button>
                  <button
                    class="btn-cancel"
                    @click="openCancelConfirmModal(booking)"
                    v-if="booking.paymentStatus !== 'cancelled' && booking.paymentStatus !== 'refunded'"
                    :disabled="isLoading"
                  >
                    取消預訂
                  </button>
                  <button
                    class="btn-rebook"
                    @click="handleRebook"
                    v-if="['cancelled', 'completed', 'refunded'].includes(booking.paymentStatus)"
                    :disabled="isLoading"
                  >
                    重新預訂
                  </button>
                  <button
                    class="btn-details"
                    data-bs-toggle="modal"
                    data-bs-target="#orderDetailModal"
                    @click="viewDetails(booking.orderNumber)"
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
  </div>

  <Teleport to="body">
    <!-- 訂單詳情 Modal -->
    <div class="modal fade" id="orderDetailModal" tabindex="-1" aria-labelledby="orderDetailModalLabel" aria-hidden="true">
      <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content" v-if="selectedBooking">
          <div class="modal-header">
            <h5 class="modal-title" id="orderDetailModalLabel">訂單詳情 #{{ selectedBooking.orderNumber }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
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
                <p>{{ formatDate(selectedBooking.checkIn) }} - {{ formatDate(selectedBooking.checkOut) }} ({{ selectedBooking.guestCount }}人)</p>
              </div>
              <div class="detail-row">
                <p><strong>訂單建立:</strong></p>
                <p>{{ formatDate(selectedBooking.createdAt) }}</p>
              </div>
              <div class="detail-row">
                <p><strong>目前狀態:</strong></p>
                <p>
                  <span :class="['order-status', `status-${selectedBooking.paymentStatus}`]">
                    {{ getStatusText(selectedBooking.paymentStatus) }}
                  </span>
                </p>
              </div>
            </div>
            <hr>
            <div class="detail-section">
              <h6><i class="fa-solid fa-credit-card"></i> 價格與付款</h6>
              <div class="detail-row">
                <p><strong>總金額:</strong></p>
                <p class="price-value">TWD {{ selectedBooking.totalPrice.toLocaleString() }}</p>
              </div>
            </div>
            <hr>
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
            <hr>
            <div class="detail-section">
              <h6><i class="fa-solid fa-address-book"></i> 帳單地址</h6>
              <div class="address-block">
                {{ selectedBooking.billingCountry }}<br>
                {{ selectedBooking.billingZipCode }} {{ selectedBooking.billingCity }}<br>
                {{ selectedBooking.billingState }} {{ selectedBooking.billingStreet }}<br>
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
    <div class="modal fade" id="cancelConfirmModal" tabindex="-1" aria-labelledby="cancelConfirmModalLabel" aria-hidden="true">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="cancelConfirmModalLabel">確認取消預訂</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            您確定要取消這筆訂單 (編號: {{ bookingToCancel?.orderNumber }}) 嗎？
            <br>
            <small class="text-muted">請注意，取消政策可能適用。</small>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isLoading">關閉</button>
            <button type="button" class="btn btn-danger" @click="confirmCancellation" :disabled="isLoading">
              <span v-if="isLoading" class="spinner-border spinner-border-sm" role="status"></span>
              <span v-else>確認取消</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" scoped>
$primary-color: #222;
$secondary-color: #008489;
$danger-color: #d9534f;
$border-color: #ebebeb;
$background-light: #f9f9f9;
$text-light: #717171;
$text-dark: #484848;

.my-bookings-page {
  padding: 40px 20px;
  background-color: $background-light;
  min-height: 100vh;
}

.container {
  max-width: 900px;
  margin: 0 auto;
}

h1 {
  margin-bottom: 30px;
  font-size: 28px;
  font-weight: 700;
  color: $primary-color;
}

.loading-spinner {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 200px;
}

.no-bookings {
  text-align: center;
  padding: 50px 20px;
  background: white;
  border-radius: 16px;
  border: 1px solid $border-color;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.04);

  p {
    font-size: 18px;
    color: $text-light;
    margin-bottom: 20px;
  }

  .btn-primary {
    padding: 12px 24px;
    background-color: $secondary-color;
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: background-color 0.2s;

    &:hover {
      background-color: darken($secondary-color, 10%);
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
    background-color: darken($secondary-color, 10%);
    border-color: darken($secondary-color, 10%);
  }
}

.btn-pay-now {
  background-color: $secondary-color;
  color: white;
  border-color: $secondary-color;
  box-shadow: 0 2px 8px rgba(0, 132, 137, 0.3);

  &:hover {
    background-color: darken($secondary-color, 10%);
    border-color: darken($secondary-color, 10%);
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
.status-unpaid { background-color: #fff3cd; color: #856404; }
.status-completed,
.status-paid { background-color: #d4edda; color: #155724; }
.status-cancelled { background-color: #f8d7da; color: #721c24; }
.status-refunded { background-color: #e2e3e5; color: #383d41; }

.modal-content {
  border-radius: 15px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);

  .modal-header {
    border-bottom: 1px solid $border-color;
    padding: 20px 25px;
    .modal-title { font-weight: 600; color: $primary-color; }
  }

  .modal-body { padding: 25px; }
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
    i { margin-right: 8px; color: $secondary-color; }
  }
}

.detail-row {
  display: grid;
  grid-template-columns: 120px 1fr;
  margin-bottom: 10px;
  font-size: 14px;
  align-items: start;

  p { margin: 0; line-height: 1.6; }
  strong { color: $text-light; font-weight: 500; }
  .price-value { font-weight: 700; color: $secondary-color; font-size: 16px; }
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

hr { border-color: $border-color; margin: 20px 0; }

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
    background-color: darken($text-light, 10%);
    border-color: darken($text-light, 10%);
  }
}

@media (max-width: 768px) {
  .container { padding: 0 15px; }
  .my-bookings-page { padding: 20px 0; }
  h1 { font-size: 24px; margin-bottom: 20px; }

  .bookings-list .booking-card {
    flex-direction: column;
    .card-image-wrapper { width: 100%; height: 200px; }
    .card-details-wrapper { padding: 16px; gap: 16px; }
    .bottom-section {
      flex-direction: column;
      align-items: stretch;
      gap: 12px;
      padding-top: 12px;
      .total-price-area { text-align: left; }
      .card-actions {
        width: 100%;
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 10px;
        .btn-pay-now { grid-column: 1 / -1; }
      }
    }
  }

  .modal-body { padding: 15px; }
  .detail-row { grid-template-columns: 90px 1fr; }
}

.btn-danger {
  background-color: $danger-color;
  border-color: $danger-color;
  color: white;

  &:hover {
    background-color: darken($danger-color, 10%);
    border-color: darken($danger-color, 10%);
  }
}
</style>
