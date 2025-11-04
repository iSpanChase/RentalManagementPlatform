<script setup>
import { ref, onMounted, computed } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';
import { useAuthStore } from '@/stores/authStore.js';
import { formatDate } from '@/composables/useBookingFormatters';
import { usePagination } from '@/composables/usePagination';
import { useOrderModal } from '@/composables/useOrderModal';
import { useCancelBookingModal } from '@/composables/useCancelBookingModal';
import SimplePaginator from '@/components/SimplePaginator.vue';
import EmptyState from '@/components/EmptyState.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import StatusBadge from '@/components/StatusBadge.vue';

const bookingStore = useBookingStore();
const router = useRouter();
const toast = useToast();
const authStore = useAuthStore();

const allBookings = ref([]);
const isLoading = ref(true);
const isError = ref(false);

// 使用共用的分頁邏輯
const { currentPage, totalPages, paginatedItems: paginatedBookings, onPageChange } = usePagination(allBookings, 5);

// 使用共用的訂單詳情 Modal 邏輯
const { selectedItem: selectedBooking, viewDetails: viewBookingDetails } = useOrderModal('orderDetailModal', allBookings);

// 使用共用的取消預訂 Modal 邏輯
const { bookingToCancel, isCancelling, openCancelConfirmModal, confirmCancellation } = useCancelBookingModal(allBookings);

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
 * 重新預訂
 */
const handleRebook = (booking) => {
  if (!booking || !booking.roomId) {
    toast.error('無法獲取房源資訊，請稍後再試');
    return;
  }
  router.push({ name: 'BookingConfirmView', query: { roomId: booking.roomId } });
};

onMounted(async () => {
  isLoading.value = true;
  isError.value = false;

  try {
    const userId = authStore.currentUser?.id;
    if (!userId) {
      toast.error('無法獲取使用者資訊，請先登入');
      isError.value = true;
      return;
    }

    const fetchedBookings = await bookingStore.fetchBookingsByUser(userId);
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

</script>

<template>
  <div class="my-bookings-page">
    <h1>我的預訂</h1>

    <!-- Loading -->
    <div v-if="isLoading || bookingStore.isLoading" class="loading-overlay">
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
                    v-if="booking.paymentStatus !== 'cancelled' && booking.paymentStatus !== 'refunded'"
                  >
                    聯繫房東
                  </button>
                  <button
                    class="btn-cancel"
                    @click="openCancelConfirmModal(booking)"
                    v-if="booking.paymentStatus !== 'cancelled' && booking.paymentStatus !== 'refunded'"
                    :disabled="isCancelling || isLoading"
                  >
                    取消預訂
                  </button>
                  <button
                    class="btn-rebook"
                    @click="handleRebook(booking)"
                    v-if="['cancelled', 'completed', 'refunded'].includes(booking.paymentStatus)"
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
                  <StatusBadge :status="selectedBooking.paymentStatus" />
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
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="isCancelling">關閉</button>
            <button type="button" class="btn btn-danger" @click="confirmCancellation" :disabled="isCancelling">
              <span v-if="isCancelling" class="spinner-border spinner-border-sm" role="status"></span>
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

// Mobile-first 設計：從最小螢幕開始設計，然後向上擴展
.my-bookings-page {
  // Mobile (320px+)
  padding: 16px;
  min-height: 100vh;
  margin: 0 auto;

  h1 {
    font-size: 24px;
    font-weight: bold;
    margin-bottom: 16px;
    color: #222;
    text-align: center;
    padding: 0 8px;
  }

  // Small mobile (375px+)
  @media (min-width: 375px) {
    padding: 20px;

    h1 {
      font-size: 26px;
      margin-bottom: 20px;
    }
  }

  // Large mobile / Small tablet (576px+)
  @media (min-width: 576px) {
    max-width: 540px;

    h1 {
      font-size: 28px;
      text-align: left;
      margin-left: 10px;
    }
  }

  // Tablet (768px+)
  @media (min-width: 768px) {
    max-width: 720px;
  }

  // Large tablet / Small desktop (992px+)
  @media (min-width: 992px) {
    max-width: 900px;
  }

  // Desktop (1200px+)
  @media (min-width: 1200px) {
    max-width: 1024px;
  }
}

.container {
  width: 100%;
  margin: 0 auto;
}

/* Loading & Error Styles - Mobile-first */
.loading-overlay, .error-message-container {
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
  // Mobile
  padding: 16px;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    padding: 20px;
  }
}

.loading-content, .error-card {
  background: white;
  border-radius: 12px;
  text-align: center;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 320px;
  // Mobile
  padding: 24px 20px;

  p {
    margin-top: 12px;
    color: #333;
    font-size: 14px;
    font-weight: 500;
    line-height: 1.4;
  }

  // Large mobile (480px+)
  @media (min-width: 480px) {
    padding: 32px 24px;
    border-radius: 16px;
    max-width: 350px;

    p {
      font-size: 16px;
      margin-top: 16px;
    }
  }

  // Tablet (768px+)
  @media (min-width: 768px) {
    padding: 40px;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  }
}

.loading-spinner {
  // Mobile
  width: 40px;
  height: 40px;
  border: 3px solid #f0f0f0;
  border-top: 3px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    width: 50px;
    height: 50px;
    border: 4px solid #f0f0f0;
    border-top: 4px solid #007bff;
  }
}

.error-card {
  h2 {
    // Mobile
    font-size: 18px;
    font-weight: 600;
    color: #d9534f;
    margin-bottom: 12px;
    line-height: 1.3;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      font-size: 20px;
      margin-bottom: 15px;
    }

    // Tablet (768px+)
    @media (min-width: 768px) {
      font-size: 22px;
    }
  }

  p {
    color: #484848;
    line-height: 1.5;

    // Tablet (768px+)
    @media (min-width: 768px) {
      line-height: 1.6;
    }
  }

  .btn-back-home {
    // Mobile
    margin-top: 16px;
    padding: 12px 20px;
    background-color: #007bff;
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: background-color 0.2s;
    width: 100%;
    font-size: 14px;

    &:hover {
      background-color: #0056b3;
    }

    // Large mobile (480px+)
    @media (min-width: 480px) {
      margin-top: 20px;
      width: auto;
      min-width: 120px;
      font-size: 16px;
      padding: 10px 20px;
    }
  }
}

.bookings-list {
  display: grid;
  // Mobile
  gap: 16px;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    gap: 20px;
  }

  // Tablet (768px+)
  @media (min-width: 768px) {
    gap: 24px;
  }

  .booking-card {
    background: white;
    border: 1px solid $border-color;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    overflow: hidden;
    transition: box-shadow 0.3s ease;
    // Mobile: 垂直布局
    display: flex;
    flex-direction: column;
    border-radius: 12px;

    &:hover {
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
    }

    // Large mobile (480px+)
    @media (min-width: 480px) {
      border-radius: 16px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);

      &:hover {
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
      }
    }

    // Tablet (768px+): 水平布局
    @media (min-width: 768px) {
      flex-direction: row;
    }

    .card-image-wrapper {
      // Mobile: 圖片在上方
      width: 100%;
      height: 200px;
      flex-shrink: 0;

      .room-image {
        width: 100%;
        height: 100%;
        object-fit: cover;
      }

      // Tablet (768px+): 圖片在左側
      @media (min-width: 768px) {
        width: 200px;
        height: auto;
      }

      // Desktop (992px+): 更寬的圖片
      @media (min-width: 992px) {
        width: 220px;
      }
    }

    .card-details-wrapper {
      flex: 1;
      display: flex;
      flex-direction: column;
      // Mobile
      padding: 16px;
      gap: 12px;

      // Large mobile (480px+)
      @media (min-width: 480px) {
        padding: 20px;
      }

      // Tablet (768px+)
      @media (min-width: 768px) {
        padding: 20px 24px;
      }
    }

    .card-section {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
    }

    .top-section {
      // Mobile: 垂直排列
      flex-direction: column;
      align-items: flex-start;
      gap: 8px;

      // Large mobile (480px+): 水平排列
      @media (min-width: 480px) {
        flex-direction: row;
        align-items: flex-start;
        gap: 0;
      }

      .room-info {
        .room-location {
          display: block;
          font-size: 14px;
          color: $text-light;
        }
        h3 {
          margin: 2px 0 0;
          // Mobile
          font-size: 18px;
          font-weight: 600;
          color: $primary-color;
          line-height: 1.3;

          // Large mobile (480px+)
          @media (min-width: 480px) {
            font-size: 20px;
          }
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
          margin-right: 8px;
          color: $text-light;
          width: 16px;
          text-align: center;

          // Large mobile (480px+)
          @media (min-width: 480px) {
            margin-right: 10px;
          }
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
      // Mobile: 垂直排列
      flex-direction: column;
      gap: 12px;

      // Large mobile (480px+): 水平排列
      @media (min-width: 480px) {
        flex-direction: row;
        gap: 0;
      }

      .total-price-area {
        // Mobile: 置中
        text-align: center;

        span {
          font-size: 13px;
          color: $text-light;
        }
        .total-price-value {
          margin: 0;
          strong {
            // Mobile
            font-size: 18px;
            font-weight: 700;
            color: $primary-color;

            // Large mobile (480px+)
            @media (min-width: 480px) {
              font-size: 20px;
            }
          }
        }

        // Large mobile (480px+): 靠左
        @media (min-width: 480px) {
          text-align: left;
        }
      }

      .card-actions {
        display: flex;
        flex-wrap: wrap;
        // Mobile: 置中
        justify-content: center;
        gap: 8px;
        width: 100%;

        // Large mobile (480px+): 靠右
        @media (min-width: 480px) {
          justify-content: flex-end;
          width: auto;
        }
      }
    }
  }
}

%btn-base {
  // Mobile
  padding: 10px 16px;
  border-radius: 8px;
  border: 1px solid;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
  min-height: 44px; // 適合觸控的最小高度
  display: flex;
  align-items: center;
  justify-content: center;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    padding: 8px 14px;
    font-size: 13px;
    min-height: auto;
  }
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

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>
