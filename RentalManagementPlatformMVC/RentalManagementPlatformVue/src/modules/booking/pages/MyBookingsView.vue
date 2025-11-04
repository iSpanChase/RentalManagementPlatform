<script setup>
import { ref, onMounted, computed } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';
import { useAuthStore } from '@/stores/auth';
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
const auth = useAuthStore();

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
      toast.error(result.message || '付款表單產生失敗');
    }
  } catch (error) {
    toast.error(error.message || '立即付款失敗');
  } finally {
    isLoading.value = false;
  }
};

/**
 * 重新導向到詳情頁面
 */
const handleViewDetails = (orderNumber) => {
  router.push({
    name: 'BookingDetail',
    params: { orderNumber }
  });
};

/**
 * 載入我的預訂
 */
onMounted(async () => {
  // 檢查使用者是否已登入
  if (!auth.isAuthenticated.value) {
    toast.error("請先登入查看預訂記錄");
    router.push({ name: "LoginView" });
    return;
  }

  isLoading.value = true;
  isError.value = false;

  try {
    const userId = auth.state.profile?.userId;
    if (!userId) {
      toast.error('無法獲取使用者資訊，請重新登入');
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
 * 重新載入資料
 */
const reloadData = async () => {
  await onMounted();
};

// 計算各種狀態的預訂數量（用於統計顯示）
const bookingStats = computed(() => {
  const stats = {
    total: allBookings.value.length,
    confirmed: 0,
    pending: 0,
    cancelled: 0,
    completed: 0
  };

  allBookings.value.forEach(booking => {
    switch (booking.status?.toLowerCase()) {
      case 'confirmed':
        stats.confirmed++;
        break;
      case 'pending':
        stats.pending++;
        break;
      case 'cancelled':
        stats.cancelled++;
        break;
      case 'completed':
        stats.completed++;
        break;
    }
  });

  return stats;
});
</script>

<template>
  <div class="my-bookings-view">
    <div class="header-section">
      <h1 class="page-title">我的預訂</h1>
      <p class="page-subtitle">管理您的住宿預訂</p>

      <!-- 統計卡片 -->
      <div class="stats-grid" v-if="!isLoading && !isError">
        <div class="stat-card">
          <div class="stat-number">{{ bookingStats.total }}</div>
          <div class="stat-label">總預訂</div>
        </div>
        <div class="stat-card">
          <div class="stat-number">{{ bookingStats.confirmed }}</div>
          <div class="stat-label">已確認</div>
        </div>
        <div class="stat-card">
          <div class="stat-number">{{ bookingStats.pending }}</div>
          <div class="stat-label">待處理</div>
        </div>
        <div class="stat-card">
          <div class="stat-number">{{ bookingStats.completed }}</div>
          <div class="stat-label">已完成</div>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <LoadingSpinner v-if="isLoading" message="載入預訂資料中..." />

    <!-- Error State -->
    <div v-else-if="isError" class="error-state">
      <EmptyState
        title="載入失敗"
        description="無法載入您的預訂資料，請重試"
        action-text="重新載入"
        @action="reloadData"
      />
    </div>

    <!-- Empty State -->
    <div v-else-if="allBookings.length === 0" class="empty-state">
      <EmptyState
        title="暫無預訂記錄"
        description="您還沒有任何預訂記錄，開始探索我們的房源吧！"
        action-text="探索房源"
        @action="router.push({ name: 'home' })"
      />
    </div>

    <!-- Bookings List -->
    <div v-else class="bookings-container">
      <div class="bookings-list">
        <div
          v-for="booking in paginatedBookings"
          :key="booking.bookingId"
          class="booking-card"
        >
          <!-- 房源圖片 -->
          <div class="booking-image">
            <img
              :src="booking.roomImage || '/placeholder-room.jpg'"
              :alt="booking.roomTitle"
              @error="$event.target.src = '/placeholder-room.jpg'"
            />
          </div>

          <!-- 預訂資訊 -->
          <div class="booking-info">
            <div class="booking-header">
              <h3 class="room-title">{{ booking.roomTitle }}</h3>
              <StatusBadge :status="booking.status" />
            </div>

            <div class="booking-details">
              <div class="detail-row">
                <span class="label">訂單編號：</span>
                <span class="value">{{ booking.orderNumber }}</span>
              </div>
              <div class="detail-row">
                <span class="label">入住日期：</span>
                <span class="value">{{ formatDate(booking.checkIn) }}</span>
              </div>
              <div class="detail-row">
                <span class="label">退房日期：</span>
                <span class="value">{{ formatDate(booking.checkOut) }}</span>
              </div>
              <div class="detail-row">
                <span class="label">住客人數：</span>
                <span class="value">{{ booking.guestCount }} 人</span>
              </div>
              <div class="detail-row">
                <span class="label">總金額：</span>
                <span class="value price">NT$ {{ booking.totalPrice?.toLocaleString() }}</span>
              </div>
            </div>
          </div>

          <!-- 操作按鈕 -->
          <div class="booking-actions">
            <button
              class="btn btn-outline"
              @click="viewBookingDetails(booking)"
            >
              查看詳情
            </button>

            <button
              v-if="booking.status === 'PendingPayment'"
              class="btn btn-primary"
              @click="handlePayNow(booking.orderNumber)"
              :disabled="isLoading"
            >
              立即付款
            </button>

            <button
              v-if="booking.status === 'Confirmed' && booking.isRefundable"
              class="btn btn-danger"
              @click="openCancelConfirmModal(booking)"
              :disabled="isCancelling"
            >
              取消預訂
            </button>
          </div>
        </div>
      </div>

      <!-- 分頁 -->
      <SimplePaginator
        v-if="totalPages > 1"
        :current-page="currentPage"
        :total-pages="totalPages"
        @page-change="onPageChange"
      />
    </div>

    <!-- 訂單詳情 Modal -->
    <div id="orderDetailModal" class="modal" tabindex="-1">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">預訂詳情</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body" v-if="selectedBooking">
            <!-- 詳細資訊內容 -->
            <div class="detail-section">
              <h6>基本資訊</h6>
              <div class="row">
                <div class="col-md-6">
                  <p><strong>訂單編號：</strong>{{ selectedBooking.orderNumber }}</p>
                  <p><strong>房源：</strong>{{ selectedBooking.roomTitle }}</p>
                  <p><strong>狀態：</strong><StatusBadge :status="selectedBooking.status" /></p>
                </div>
                <div class="col-md-6">
                  <p><strong>入住：</strong>{{ formatDate(selectedBooking.checkIn) }}</p>
                  <p><strong>退房：</strong>{{ formatDate(selectedBooking.checkOut) }}</p>
                  <p><strong>人數：</strong>{{ selectedBooking.guestCount }} 人</p>
                </div>
              </div>
            </div>

            <div class="detail-section" v-if="selectedBooking.billingInfo">
              <h6>聯絡資訊</h6>
              <p><strong>姓名：</strong>{{ selectedBooking.billingInfo.name }}</p>
              <p><strong>Email：</strong>{{ selectedBooking.billingInfo.email }}</p>
              <p><strong>電話：</strong>{{ selectedBooking.billingInfo.phone }}</p>
            </div>

            <div class="detail-section">
              <h6>費用明細</h6>
              <div class="price-breakdown">
                <div class="price-row">
                  <span>房費 ({{ selectedBooking.nights }}晚)</span>
                  <span>NT$ {{ selectedBooking.subtotal?.toLocaleString() }}</span>
                </div>
                <div class="price-row">
                  <span>服務費</span>
                  <span>NT$ {{ selectedBooking.serviceFee?.toLocaleString() }}</span>
                </div>
                <div class="price-row total">
                  <span>總計</span>
                  <span>NT$ {{ selectedBooking.totalPrice?.toLocaleString() }}</span>
                </div>
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
    <div id="cancelConfirmModal" class="modal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">確認取消預訂</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body" v-if="bookingToCancel">
            <p>您確定要取消以下預訂嗎？</p>
            <div class="cancel-booking-info">
              <p><strong>房源：</strong>{{ bookingToCancel.roomTitle }}</p>
              <p><strong>訂單編號：</strong>{{ bookingToCancel.orderNumber }}</p>
              <p><strong>入住日期：</strong>{{ formatDate(bookingToCancel.checkIn) }}</p>
              <p><strong>總金額：</strong>NT$ {{ bookingToCancel.totalPrice?.toLocaleString() }}</p>
            </div>
            <div class="alert alert-warning">
              <small>取消後將依照退款政策進行退款處理</small>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">保留預訂</button>
            <button
              type="button"
              class="btn btn-danger"
              @click="confirmCancellation"
              :disabled="isCancelling"
            >
              {{ isCancelling ? '取消中...' : '確認取消' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.my-bookings-view {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;

  @media (max-width: 768px) {
    padding: 16px;
  }
}

.header-section {
  margin-bottom: 32px;

  .page-title {
    font-size: 32px;
    font-weight: 700;
    color: #222;
    margin-bottom: 8px;

    @media (max-width: 768px) {
      font-size: 24px;
    }
  }

  .page-subtitle {
    color: #717171;
    font-size: 16px;
    margin-bottom: 24px;
  }
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 16px;
  margin-bottom: 24px;

  .stat-card {
    background: white;
    border: 1px solid #ebebeb;
    border-radius: 12px;
    padding: 20px;
    text-align: center;

    .stat-number {
      font-size: 24px;
      font-weight: 700;
      color: #222;
      margin-bottom: 4px;
    }

    .stat-label {
      font-size: 14px;
      color: #717171;
    }
  }
}

.bookings-container {
  .bookings-list {
    display: flex;
    flex-direction: column;
    gap: 20px;
    margin-bottom: 32px;
  }
}

.booking-card {
  background: white;
  border: 1px solid #ebebeb;
  border-radius: 16px;
  overflow: hidden;
  display: grid;
  grid-template-columns: 200px 1fr auto;
  transition: all 0.2s ease;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    grid-template-rows: 200px auto auto;
  }

  .booking-image {
    img {
      width: 100%;
      height: 200px;
      object-fit: cover;
    }
  }

  .booking-info {
    padding: 20px;

    .booking-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 16px;

      .room-title {
        font-size: 18px;
        font-weight: 600;
        color: #222;
        margin: 0;
        flex: 1;
        margin-right: 12px;
      }
    }

    .booking-details {
      .detail-row {
        display: flex;
        justify-content: space-between;
        margin-bottom: 8px;

        .label {
          color: #717171;
          font-size: 14px;
        }

        .value {
          font-weight: 500;
          color: #222;

          &.price {
            color: #ff5a5f;
            font-weight: 600;
          }
        }
      }
    }
  }

  .booking-actions {
    padding: 20px;
    display: flex;
    flex-direction: column;
    gap: 8px;
    justify-content: center;

    @media (max-width: 768px) {
      flex-direction: row;
    }

    .btn {
      padding: 8px 16px;
      border-radius: 8px;
      font-weight: 600;
      text-decoration: none;
      border: none;
      cursor: pointer;
      transition: all 0.2s;
      font-size: 14px;

      &.btn-outline {
        background: white;
        color: #222;
        border: 1px solid #222;

        &:hover {
          background: #222;
          color: white;
        }
      }

      &.btn-primary {
        background: #ff5a5f;
        color: white;

        &:hover {
          background: #e04348;
        }

        &:disabled {
          background: #ccc;
          cursor: not-allowed;
        }
      }

      &.btn-danger {
        background: #dc3545;
        color: white;

        &:hover {
          background: #c82333;
        }

        &:disabled {
          background: #ccc;
          cursor: not-allowed;
        }
      }
    }
  }
}

.error-state,
.empty-state {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 400px;
}

.detail-section {
  margin-bottom: 24px;

  h6 {
    font-weight: 600;
    color: #222;
    margin-bottom: 12px;
    padding-bottom: 8px;
    border-bottom: 1px solid #ebebeb;
  }

  p {
    margin-bottom: 8px;
    font-size: 14px;
  }
}

.price-breakdown {
  .price-row {
    display: flex;
    justify-content: space-between;
    padding: 8px 0;
    border-bottom: 1px solid #f7f7f7;

    &.total {
      font-weight: 600;
      font-size: 16px;
      border-bottom: none;
      border-top: 2px solid #222;
      margin-top: 8px;
      padding-top: 12px;
    }
  }
}

.cancel-booking-info {
  background: #f8f9fa;
  padding: 16px;
  border-radius: 8px;
  margin: 16px 0;

  p {
    margin-bottom: 8px;
    font-size: 14px;

    &:last-child {
      margin-bottom: 0;
    }
  }
}
</style>