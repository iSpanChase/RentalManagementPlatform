<script setup>
import { ref, onMounted, computed } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useAuthStore } from '@/stores/authStore.js';
import { useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';
import { formatDate } from '@/composables/useBookingFormatters';
import { usePagination } from '@/composables/usePagination';
import { useOrderModal } from '@/composables/useOrderModal';
import SimplePaginator from '@/components/SimplePaginator.vue';
import StatusBadge from '@/components/StatusBadge.vue';
import EmptyState from '@/components/EmptyState.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

const bookingStore = useBookingStore();
const authStore = useAuthStore();
const router = useRouter();
const toast = useToast();

const allOrders = ref([]);
const isLoading = ref(true);
const isError = ref(false);

// 使用共用的分頁邏輯
const { currentPage, totalPages, paginatedItems: paginatedOrders, onPageChange } = usePagination(allOrders, 5);

// 使用共用的 Modal 邏輯
const { selectedItem: selectedOrder, viewDetails } = useOrderModal('orderDetailModal');

/**
 * 開啟訂單詳情 Modal
 */
const handleViewDetails = (orderNumber) => {
  viewDetails(orderNumber, allOrders.value);
};

onMounted(async () => {
  isLoading.value = true;
  isError.value = false;

  try {
    const hostId = authStore.currentHostId;
    if (!hostId) {
      toast.error('無法獲取房東資訊，請確認您的帳號是否為房東');
      isError.value = true;
      return;
    }

    const fetchedOrders = await bookingStore.fetchOrdersByHost(hostId);
    allOrders.value = fetchedOrders || [];

    if (allOrders.value.length === 0) {
      toast.info('目前沒有房客預訂記錄');
    }
  } catch (error) {
    console.error('載入訂單失敗:', error);
    toast.error(error.message || '載入訂單資料失敗');
    isError.value = true;
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="host-orders-page">
    <h1>房客預訂清單</h1>

    <!-- Loading -->
    <div v-if="isLoading || bookingStore.isLoading" class="loading-overlay">
      <div class="loading-content">
        <div class="loading-spinner"></div>
        <p>{{ bookingStore.isLoading ? '正在處理訂單...' : '正在載入房客預訂資料...' }}</p>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="isError" class="error-message-container">
      <div class="error-card">
        <h2>無法載入頁面</h2>
        <p>抱歉，載入房客預訂資料時發生錯誤，請確認您的房東權限。</p>
        <button @click="router.push({ name: 'home' })" class="btn-back-home">返回首頁</button>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="allOrders.length === 0" class="error-message-container">
      <div class="error-card">
        <h2>尚無房客預訂</h2>
        <p>目前沒有任何房客預訂您的房源。</p>
        <button @click="router.push({ name: 'home' })" class="btn-back-home">前往房源管理</button>
      </div>
    </div>

    <!-- Content -->
    <div v-else class="container">
      <div class="orders-list">
        <div v-for="order in paginatedOrders" :key="order.orderNumber" class="order-card">
            <div class="guest-avatar-wrapper">
              <img
                :src="order.guestAvatarUrl || 'https://placehold.co/80x80/EBEBEB/717171?text=Guest'"
                alt="房客頭像"
                class="guest-avatar"
              />
            </div>

            <div class="order-details-wrapper">
              <div class="card-section top-section">
                <div class="guest-info">
                  <h3>{{ order.guestName }} ({{ order.guestCount }}位)</h3>
                </div>
                <StatusBadge :status="order.paymentStatus" />
              </div>

              <div class="card-section mid-section">
                <div class="info-item date-info">
                  <i class="fa-solid fa-calendar-days"></i>
                  <span>{{ formatDate(order.checkIn) }} - {{ formatDate(order.checkOut) }}</span>
                </div>
                <div class="info-item room-info">
                  <i class="fa-solid fa-house"></i>
                  <span>{{ order.room }}</span>
                </div>
              </div>

              <div class="card-section bottom-section">
                <div class="price-preview">
                  <strong>TWD {{ order.totalPrice.toLocaleString() }}</strong>
                </div>
                <button
                  class="btn-details"
                  data-bs-toggle="modal"
                  data-bs-target="#orderDetailModal"
                  @click="handleViewDetails(order.orderNumber)"
                >
                  查看詳情 / 聯絡
                </button>
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

  <!-- 訂單詳情 Modal -->
  <Teleport to="body">
    <div
      class="modal fade"
      id="orderDetailModal"
      tabindex="-1"
      aria-labelledby="orderDetailModalLabel"
      aria-hidden="true"
    >
      <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content" v-if="selectedOrder">
          <div class="modal-header">
            <div class="modal-header-content">
              <img
                :src="selectedOrder.guestAvatarUrl || 'https://placehold.co/60x60/EBEBEB/717171?text=Guest'"
                alt="房客頭像"
                class="modal-avatar"
              />
              <div>
                <h5 class="modal-title" id="orderDetailModalLabel">
                  {{ selectedOrder.guestName }}
                </h5>
                <span class="modal-subtitle">訂單 #{{ selectedOrder.orderNumber }}</span>
              </div>
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>

          <div class="modal-body">
            <!-- 房客聯絡資訊（高亮） -->
            <div class="detail-section highlight">
              <h6><i class="fa-solid fa-user"></i> 房客聯絡人資訊</h6>
              <div class="detail-row">
                <p><strong>Email:</strong></p>
                <p>{{ selectedOrder.contactEmail }}</p>
              </div>
              <div class="detail-row">
                <p><strong>電話:</strong></p>
                <p>{{ selectedOrder.contactPhone }}</p>
              </div>
            </div>

            <hr />

            <!-- 預訂與入住細節 -->
            <div class="detail-section">
              <h6><i class="fa-solid fa-house"></i> 預訂與入住細節</h6>
              <div class="detail-row">
                <p><strong>房源名稱:</strong></p>
                <p>{{ selectedOrder.room }}</p>
              </div>
              <div class="detail-row">
                <p><strong>入住/退房:</strong></p>
                <p>{{ formatDate(selectedOrder.checkIn) }} - {{ formatDate(selectedOrder.checkOut) }}</p>
              </div>
              <div class="detail-row">
                <p><strong>入住人數:</strong></p>
                <p>{{ selectedOrder.guestCount }} 位</p>
              </div>
              <div class="detail-row">
                <p><strong>訂單狀態:</strong></p>
                <p>
                  <StatusBadge :status="selectedOrder.paymentStatus" />
                </p>
              </div>
              <div class="detail-row" v-if="selectedOrder.contactNotes">
                <p><strong>房客備註:</strong></p>
                <p class="notes-text">{{ selectedOrder.contactNotes }}</p>
              </div>
            </div>

            <hr />

            <!-- 價格與款項 -->
            <div class="detail-section">
              <h6><i class="fa-solid fa-credit-card"></i> 價格與款項</h6>
              <div class="detail-row">
                <p><strong>總預訂金額:</strong></p>
                <p class="price-value">TWD {{ selectedOrder.totalPrice.toLocaleString() }}</p>
              </div>
              <div class="detail-row">
                <p><strong>您的淨收入:</strong></p>
                <p class="net-income-value">
                  <strong>TWD {{ Math.round(selectedOrder.totalPrice * 0.85).toLocaleString() }}</strong>
                  <span class="text-muted"> (15% 平台佣金)</span>
                </p>
              </div>
              <div class="detail-row">
                <p><strong>付款方式:</strong></p>
                <p>{{ selectedOrder.paymentTiming === 'full' ? '立即支付' : '延後支付' }}</p>
              </div>
              <div class="detail-row warning" v-if="selectedOrder.paymentStatus === 'deferred'">
                <p><strong>付款截止日:</strong></p>
                <p>{{ formatDate(selectedOrder.paymentDeadline) }}</p>
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">關閉</button>
            <button type="button" class="btn btn-chat">
              <i class="fa-solid fa-comments"></i> 聯絡房客
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" scoped>
$primary-color: #222;
$secondary-color: #f7a800;
$chat-color: #008489;
$border-color: #ebebeb;
$background-light: #f9f9f9;
$text-light: #717171;
$text-dark: #484848;

// Mobile-first 設計：從最小螢幕開始設計，然後向上擴展
.host-orders-page {
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

.orders-list {
  display: grid;
  // Mobile
  gap: 16px;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    gap: 20px;
  }

  .order-card {
    background: white;
    border: 1px solid $border-color;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    transition: box-shadow 0.3s ease;
    // Mobile: 垂直布局
    display: flex;
    flex-direction: column;
    border-radius: 12px;
    padding: 16px;
    gap: 16px;

    &:hover {
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
    }

    // Large mobile (480px+)
    @media (min-width: 480px) {
      border-radius: 16px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
      padding: 20px;
      gap: 20px;

      &:hover {
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
      }
    }

    // Tablet (768px+): 水平布局
    @media (min-width: 768px) {
      flex-direction: row;
      align-items: center;
    }

    .guest-avatar-wrapper {
      // Mobile: 頭像在上方，居中
      align-self: center;
      flex-shrink: 0;

      .guest-avatar {
        // Mobile
        width: 60px;
        height: 60px;
        border-radius: 50%;
        object-fit: cover;
        border: 2px solid $border-color;

        // Large mobile (480px+)
        @media (min-width: 480px) {
          width: 80px;
          height: 80px;
        }
      }

      // Tablet (768px+): 頭像在左側
      @media (min-width: 768px) {
        align-self: flex-start;
      }
    }

    .order-details-wrapper {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .card-section {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
    }

    .top-section {
      // Mobile: 垂直排列
      flex-direction: column;
      align-items: center;
      gap: 8px;
      text-align: center;

      .guest-info h3 {
        margin: 0;
        // Mobile
        font-size: 18px;
        font-weight: 600;
        color: $primary-color;

        // Large mobile (480px+)
        @media (min-width: 480px) {
          font-size: 20px;
        }
      }

      // Large mobile (480px+): 水平排列
      @media (min-width: 480px) {
        flex-direction: row;
        align-items: flex-start;
        text-align: left;
        gap: 0;
      }
    }

    .mid-section {
      flex-direction: column;
      gap: 8px;
      padding-bottom: 10px;
      border-bottom: 1px dashed $border-color;

      .info-item {
        display: flex;
        align-items: center;
        color: $text-dark;
        font-size: 14px;
        // Mobile: 居中對齊
        justify-content: center;

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

        &.date-info {
          font-weight: 500;
          // Large mobile (480px+)
          @media (min-width: 480px) {
            font-size: 15px;
          }
        }
        &.room-info {
          font-size: 13px;
          color: $text-light;

          // Large mobile (480px+)
          @media (min-width: 480px) {
            font-size: 14px;
          }
        }

        // Large mobile (480px+): 靠左對齊
        @media (min-width: 480px) {
          justify-content: flex-start;
        }
      }
    }

    .bottom-section {
      align-items: center;
      // Mobile: 垂直排列
      flex-direction: column;
      gap: 12px;
      text-align: center;

      .price-preview {
        // Mobile
        font-size: 16px;
        font-weight: 600;
        color: $primary-color;
      }

      // Large mobile (480px+): 水平排列
      @media (min-width: 480px) {
        flex-direction: row;
        gap: 0;
        text-align: left;
      }
    }
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

.status-deferred, .status-unpaid   { background-color: #fff3cd; color: #856404; }
.status-completed, .status-paid    { background-color: #d4edda; color: #155724; }
.status-cancelled                  { background-color: #f8d7da; color: #721c24; }
.status-refunded                   { background-color: #e2e3e5; color: #383d41; }

.btn-details {
  background-color: $secondary-color;
  color: $primary-color;
  border: 1px solid darken($secondary-color, 10%);
  border-radius: 8px;
  font-weight: 600;
  transition: all 0.2s;
  // Mobile
  padding: 12px 20px;
  font-size: 14px;
  width: 100%;
  min-height: 44px; // 適合觸控的最小高度
  display: flex;
  align-items: center;
  justify-content: center;

  &:hover {
    background-color: darken($secondary-color, 10%);
    border-color: darken($secondary-color, 15%);
  }

  // Large mobile (480px+)
  @media (min-width: 480px) {
    padding: 10px 18px;
    width: auto;
    min-height: auto;
  }
}

.modal-content {
  border-radius: 15px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
}

.modal-header {
  padding: 20px 25px;
  border-bottom: 1px solid $border-color;
  align-items: center;

  .modal-header-content {
    display: flex;
    align-items: center;
    gap: 15px;
    flex-grow: 1;
  }

  .modal-avatar {
    width: 60px;
    height: 60px;
    border-radius: 50%;
    object-fit: cover;
    border: 2px solid $border-color;
  }

  .modal-title {
    font-weight: 600;
    color: $primary-color;
    margin-bottom: 2px;
  }

  .modal-subtitle {
    font-size: 13px;
    color: $text-light;
  }

  .btn-close {
    margin: -10px -10px -10px 10px;
  }
}

.modal-body { padding: 25px; }

.detail-section {
  margin-bottom: 20px;
  h6 {
    font-size: 16px;
    font-weight: 700;
    margin-bottom: 15px;
    color: $primary-color;
    i { margin-right: 8px; color: $text-light; }
  }
}

.detail-section.highlight {
  background-color: #fffaf0;
  padding: 20px;
  border-radius: 10px;
  border: 1px solid $secondary-color;
  margin-top: -10px;

  h6 {
    border-bottom: 1px solid #feedcd;
    padding-bottom: 10px;
    i { color: $secondary-color; }
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

  .price-value {
    font-weight: 700;
    color: $primary-color;
    font-size: 15px;
  }

  .net-income-value {
    color: #155724;
    font-weight: 500;
    strong { color: #155724; font-weight: 700; font-size: 15px; }
    .text-muted { font-weight: normal; color: $text-light !important; font-size: 13px; }
  }

  .notes-text {
    background-color: #f7f7f7;
    padding: 10px;
    border-radius: 8px;
    color: $primary-color;
    white-space: pre-wrap;
    font-style: italic;
  }

  &.warning p:last-child {
    color: #e63946;
    font-weight: 600;
  }
}

hr { border-color: $border-color; margin: 25px 0; }

.modal-footer {
  border-top: 1px solid $border-color;
  padding: 15px 25px;

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

  .btn-chat {
    background-color: $chat-color;
    color: white;
    border: 1px solid $chat-color;
    padding: 8px 20px;
    border-radius: 8px;
    font-weight: 600;
    transition: all 0.2s;
    i { margin-right: 6px; }
    &:hover {
      background-color: darken($chat-color, 10%);
      border-color: darken($chat-color, 10%);
    }
  }
}

@media (max-width: 768px) {
  .container { padding: 0 15px; }
  .host-orders-page { padding: 20px 0; }
  h1 { font-size: 24px; margin-bottom: 20px; }

  .orders-list .order-card {
    flex-direction: column;
    align-items: center;
    padding: 16px;
    gap: 15px;

    .order-details-wrapper { width: 100%; }

    .top-section {
      flex-direction: column;
      align-items: flex-start;
      gap: 8px;
    }

    .bottom-section {
      flex-direction: column;
      align-items: stretch;
      gap: 10px;
      .price-preview { text-align: center; }
      .btn-details { width: 100%; }
    }
  }

  .modal-body { padding: 15px; }
  .detail-section.highlight { padding: 15px; }
  .detail-row { grid-template-columns: 90px 1fr; }
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>

<!--
  注意：本元件使用 Font Awesome 圖示，需確保專案已引入：
  <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet">
-->
