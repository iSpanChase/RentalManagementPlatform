<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useRoute } from 'vue-router';
import { Modal } from 'bootstrap';

// ==================== 狀態宣告 ====================
const bookingStore = useBookingStore();
const route = useRoute();

const orders = ref([]);
const isLoading = ref(false);
const selectedOrder = ref(null);
const testHostId = 47;

// ==================== 工具函數 ====================
/**
 * 格式化日期為「2025年10月29日」格式
 * @param {string} dateStr - ISO 日期字串
 * @returns {string}
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
 * 取得訂單狀態文字
 * @param {string} status
 * @returns {string}
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
 * 開啟訂單詳情 Modal
 * @param {string} orderNumber
 */
const viewDetails = (orderNumber) => {
  const order = orders.value.find(b => b.orderNumber === orderNumber);
  if (!order) {
    console.error(`找不到訂單 ${orderNumber}`);
    return;
  }
  selectedOrder.value = order;
  console.log(`設置訂單 ${orderNumber} 資料，準備開啟 Modal`);
};

/**
 * Modal 隱藏後清除資料
 */
const handleModalHidden = () => {
  console.log('Modal 已隱藏，清除 selectedOrder 資料');
  selectedOrder.value = null;
};

// ==================== 生命週期 ====================
onMounted(async () => {
  isLoading.value = true;

  // 綁定 Bootstrap Modal 隱藏事件
  const modalElement = document.getElementById('orderDetailModal');
  if (modalElement) {
    modalElement.addEventListener('hidden.bs.modal', handleModalHidden);
  }

  try {
    const orderNumberFromQuery = route.query.orderNumber;
    let fetchedOrders = [];

    if (orderNumberFromQuery) {
      console.log(`URL 中檢測到訂單編號: ${orderNumberFromQuery}，正在獲取單一訂單詳情...`);
      const singleOrder = await bookingStore.fetchBookingByOrderNumber(orderNumberFromQuery);
      if (singleOrder) {
        fetchedOrders.push(singleOrder);
      } else {
        console.warn(`找不到訂單編號為 ${orderNumberFromQuery} 的訂單。`);
      }
    } else {
      // 取得 hostId，無則使用測試 ID
      let hostId = route.query.hostId;
      if (!hostId) {
        console.warn('URL 中未提供 hostId，使用測試房東 ID');
        hostId = testHostId;
      }
      console.log(`URL 中未提供訂單編號，正在獲取房東 ${hostId} 的所有訂單...`);
      fetchedOrders = await bookingStore.fetchHostOrders(hostId);
    }

    orders.value = fetchedOrders;

    console.log('從後端獲取訂單成功:', orders.value);
  } catch (error) {
    console.error('獲取訂單失敗:', error);
    alert('無法載入訂單資料');
  } finally {
    isLoading.value = false;
  }
});

onUnmounted(() => {
  const modalElement = document.getElementById('orderDetailModal');
  if (modalElement) {
    modalElement.removeEventListener('hidden.bs.modal', handleModalHidden);
  }
});
</script>

<template>
  <div class="host-orders-page">
    <div class="container">
      <h1>房客預訂清單</h1>

      <!-- 載入中 -->
      <div v-if="isLoading" class="loading-spinner">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">載入中...</span>
        </div>
      </div>

      <!-- 無訂單 -->
      <div v-else-if="orders.length === 0" class="no-orders">
        <p>目前沒有任何房客預訂。</p>
        <router-link to="/host/listings">
          <button class="btn-primary">前往房源管理</button>
        </router-link>
      </div>

      <!-- 訂單列表 -->
      <div v-else class="orders-list">
        <div
          v-for="order in orders"
          :key="order.orderNumber"
          class="order-card"
        >
          <!-- 房客頭像 -->
          <div class="guest-avatar-wrapper">
            <img
              :src="order.guestAvatarUrl || 'https://placehold.co/80x80/EBEBEB/717171?text=Guest'"
              alt="房客頭像"
              class="guest-avatar"
            />
          </div>

          <!-- 訂單內容 -->
          <div class="order-details-wrapper">
            <!-- 頂部：姓名 + 狀態 -->
            <div class="card-section top-section">
              <div class="guest-info">
                <h3>{{ order.guestName }} ({{ order.guestCount }}位)</h3>
              </div>
              <span :class="['order-status', `status-${order.paymentStatus}`]">
                {{ getStatusText(order.paymentStatus) }}
              </span>
            </div>

            <!-- 中間：日期 + 房源 -->
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

            <!-- 底部：價格 + 按鈕 -->
            <div class="card-section bottom-section">
              <div class="price-preview">
                <strong>TWD {{ order.totalPrice.toLocaleString() }}</strong>
              </div>
              <button
                class="btn-details"
                data-bs-toggle="modal"
                data-bs-target="#orderDetailModal"
                @click="viewDetails(order.orderNumber)"
              >
                查看詳情 / 聯絡
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- Modal：訂單詳情 -->
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
          <!-- Modal Header -->
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

          <!-- Modal Body -->
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
                  <span :class="['order-status', `status-${selectedOrder.paymentStatus}`]">
                    {{ getStatusText(selectedOrder.paymentStatus) }}
                  </span>
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

          <!-- Modal Footer -->
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
              關閉
            </button>
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
// ==================== 顏色變數 ====================
$primary-color: #222;
$secondary-color: #f7a800;     // 房東強調色（橘黃）
$chat-color: #008489;         // 溝通按鈕色
$border-color: #ebebeb;
$background-light: #f9f9f9;
$text-light: #717171;
$text-dark: #484848;

// ==================== 頁面基礎 ====================
.host-orders-page {
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

// ==================== 載入 & 無資料 ====================
.loading-spinner {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 200px;
}

.no-orders {
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
    color: $primary-color;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    transition: background-color 0.2s;

    &:hover {
      background-color: darken($secondary-color, 10%);
    }
  }
}

// ==================== 訂單卡片 ====================
.orders-list {
  display: grid;
  gap: 20px;

  .order-card {
    display: flex;
    align-items: center;
    background: white;
    border: 1px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
    padding: 20px;
    gap: 20px;
    transition: box-shadow 0.3s ease;

    &:hover {
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
    }

    // 頭像
    .guest-avatar-wrapper {
      flex-shrink: 0;

      .guest-avatar {
        width: 80px;
        height: 80px;
        border-radius: 50%;
        object-fit: cover;
        border: 2px solid $border-color;
      }
    }

    // 內容區
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

    .top-section .guest-info h3 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
      color: $primary-color;
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
        font-size: 15px;

        i {
          margin-right: 10px;
          color: $text-light;
          width: 16px;
          text-align: center;
        }

        &.date-info { font-weight: 500; }
        &.room-info { font-size: 14px; color: $text-light; }
      }
    }

    .bottom-section {
      align-items: center;

      .price-preview {
        font-size: 16px;
        font-weight: 600;
        color: $primary-color;
      }
    }
  }
}

// ==================== 狀態標籤 ====================
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

// ==================== 按鈕 ====================
.btn-details {
  background-color: $secondary-color;
  color: $primary-color;
  border: 1px solid darken($secondary-color, 10%);
  padding: 10px 18px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 14px;
  transition: all 0.2s;

  &:hover {
    background-color: darken($secondary-color, 10%);
    border-color: darken($secondary-color, 15%);
  }
}

// ==================== Modal ====================
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

    i {
      margin-right: 8px;
      color: $text-light;
    }
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

hr {
  border-color: $border-color;
  margin: 25px 0;
}

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

// ==================== 響應式 ====================
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
</style>

<!--
  注意：本元件使用 Font Awesome 圖示，需確保專案已引入：
  <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet">
-->
