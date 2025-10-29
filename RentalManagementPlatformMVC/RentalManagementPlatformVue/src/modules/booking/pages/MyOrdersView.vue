<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useRoute } from 'vue-router';
// 匯入 Modal 類別，用於事件監聽
import { Modal } from 'bootstrap';

const bookingStore = useBookingStore();
const bookings = ref([]);
const isLoading = ref(false);
const route = useRoute();
const testHostId = 99; // 假設這是房東的 ID
const selectedBooking = ref(null);

// ==================== 工具函數 ====================
/**
 * 格式化日期
 * @param {string} dateStr - ISO 日期字串
 * @param {boolean} includeTime - 是否包含時間
 * @returns {string} 格式化後的日期
 */
const formatDate = (dateStr, includeTime = false) => {
  if (!dateStr) return 'N/A';
  const date = new Date(dateStr);
  if (isNaN(date)) return 'N/A';

  const options = {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  };
  if (includeTime) {
    options.hour = '2-digit';
    options.minute = '2-digit';
    options.hour12 = false; // 使用 24 小時制
  }
  return date.toLocaleDateString('zh-TW', options) + (includeTime ? ' ' + date.toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit', hour12: false }) : '');
};

// 取得狀態文字
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

// 查看訂單詳情
const viewDetails = (orderNumber) => {
  const booking = bookings.value.find(b => b.orderNumber === orderNumber);
  if (!booking) {
    console.error(`找不到訂單 ${orderNumber}`);
    return;
  }
  selectedBooking.value = booking;
  console.log(`設置訂單 ${orderNumber} 資料，準備開啟 Modal`);
};

// 處理 Modal 隱藏事件
const handleModalHidden = () => {
    console.log('Modal 已隱藏，清除 selectedBooking 資料');
    selectedBooking.value = null;
};

onMounted(async () => {
  isLoading.value = true;

  // ==================== Modal 事件監聽設置 ====================
  const modalElement = document.getElementById('orderDetailModal');
  if (modalElement) {
    // ⚠️ 確保 bootstrap 已經被載入
    modalElement.addEventListener('hidden.bs.modal', handleModalHidden);
  }

  try {
    let hostId = route.query.hostId;
    if (!hostId) {
      console.warn('URL 中未提供 hostId，使用測試房東 ID');
      hostId = testHostId;
    }

    // 實際應用中請呼叫後端 API 獲取數據
    // const fetchedBookings = await bookingStore.fetchHostBookings(hostId);

    // *** Mock 數據以符合房東視角 (保留您的範例數據) ***
    const mockBookings = [
      {
        orderNumber: 20250001,
        room: '山景豪華雙人房 A101',
        checkIn: new Date(new Date().getTime() + 1 * 24 * 60 * 60 * 1000).toISOString(),
        checkOut: new Date(new Date().getTime() + 3 * 24 * 60 * 60 * 1000).toISOString(),
        totalPrice: 8500,
        paymentStatus: 'completed',
        guestCount: 2,
        guestName: '林志玲',
        contactName: '林志玲',
        contactEmail: 'chiling.lin@example.com',
        contactPhone: '0910-123-456',
        contactNotes: '需要安靜的角落房，謝謝。',
        createdAt: new Date(new Date().getTime() - 10 * 24 * 60 * 60 * 1000).toISOString(),
        pointsRedeemed: 0,
        paymentTiming: 'full',
        paymentDeadline: null,
        billingCountry: '臺灣', billingStreet: '信義路五段', billingApartment: '101號', billingCity: '台北市', billingState: 'N/A', billingZipCode: '110',
      },
      {
        orderNumber: 20250002,
        room: '海景四人家庭房 B202',
        checkIn: new Date(new Date().getTime() + 10 * 24 * 60 * 60 * 1000).toISOString(),
        checkOut: new Date(new Date().getTime() + 15 * 24 * 60 * 60 * 1000).toISOString(),
        totalPrice: 15000,
        paymentStatus: 'deferred',
        guestCount: 4,
        guestName: '周杰倫',
        contactName: '周杰倫',
        contactEmail: 'jay.chou@example.com',
        contactPhone: '0920-789-000',
        contactNotes: '兩大兩小，請準備兒童用品。',
        createdAt: new Date(new Date().getTime() - 5 * 24 * 60 * 60 * 1000).toISOString(),
        pointsRedeemed: 1500,
        paymentTiming: 'partial',
        paymentDeadline: new Date(new Date().getTime() + 5 * 24 * 60 * 60 * 1000).toISOString(),
        billingCountry: '臺灣', billingStreet: '仁愛路二段', billingApartment: '88號', billingCity: '台北市', billingState: 'N/A', billingZipCode: '106',
      },
    ];
    bookings.value = mockBookings;
    // *** Mock 結束 ***

    console.log('從後端獲取訂單成功:', bookings.value);
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
  <div class="host-bookings-page">
    <div class="container">
      <h1>房客預訂清單</h1>

      <div v-if="isLoading" class="loading-spinner">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">載入中...</span>
        </div>
      </div>

      <div v-else-if="bookings.length === 0" class="no-bookings">
        <p>目前沒有任何房客預訂。</p>
        <router-link to="/host/listings">
          <button class="btn-primary">前往房源管理頁</button>
        </router-link>
      </div>

      <div v-else class="bookings-list">
        <div v-for="booking in bookings" :key="booking.orderNumber" class="booking-card">
          <div class="card-header">
            <h3>{{ booking.room }}</h3>
            <span :class="['order-status', `status-${booking.paymentStatus}`]">
              {{ getStatusText(booking.paymentStatus) }}
            </span>
          </div>
          <div class="card-body">
            <div class="order-info-grid">
              <p><strong>房客姓名:</strong></p>
              <p class="highlight-info">{{ booking.guestName }}</p>

              <p><strong>入住/退房:</strong></p>
              <p>{{ formatDate(booking.checkIn) }} - {{ formatDate(booking.checkOut) }}</p>

              <p><strong>入住人數:</strong></p>
              <p>{{ booking.guestCount }} 位</p>

              <p class="total-price"><strong>總金額:</strong></p>
              <p class="total-price-value">
                <strong>TWD {{ booking.totalPrice.toLocaleString() }}</strong>
              </p>
            </div>
          </div>
          <div class="card-footer">
            <button
              class="btn-details"
              data-bs-toggle="modal"
              data-bs-target="#orderDetailModal"
              @click="viewDetails(booking.orderNumber)"
            >
              查看詳情/聯絡房客
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>

  <Teleport to="body">
    <div class="modal fade" id="orderDetailModal" tabindex="-1" aria-labelledby="orderDetailModalLabel" aria-hidden="true">
      <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content" v-if="selectedBooking">
          <div class="modal-header">
            <h5 class="modal-title" id="orderDetailModalLabel">訂單詳情 #{{ selectedBooking.orderNumber }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>

          <div class="modal-body">
            <div class="detail-section highlight">
              <h6>👤 房客聯絡人資訊</h6>
              <div class="detail-row">
                <p><strong>訂房人:</strong></p>
                <p class="text-large">{{ selectedBooking.guestName }} ({{ selectedBooking.guestCount }}位)</p>
              </div>
              <div class="detail-row">
                <p><strong>Email:</strong></p>
                <p>{{ selectedBooking.contactEmail }}</p>
              </div>
              <div class="detail-row">
                <p><strong>電話:</strong></p>
                <p>{{ selectedBooking.contactPhone }}</p>
              </div>
            </div>

            <hr>

            <div class="detail-section">
              <h6>🏠 預訂與入住細節</h6>
              <div class="detail-row">
                <p><strong>房源名稱:</strong></p>
                <p>{{ selectedBooking.room }}</p>
              </div>
              <div class="detail-row">
                <p><strong>入住/退房:</strong></p>
                <p>{{ formatDate(selectedBooking.checkIn) }} - {{ formatDate(selectedBooking.checkOut) }}</p>
              </div>
              <div class="detail-row">
                <p><strong>訂單狀態:</strong></p>
                <p>
                  <span :class="['order-status', `status-${selectedBooking.paymentStatus}`]">
                    {{ getStatusText(selectedBooking.paymentStatus) }}
                  </span>
                </p>
              </div>
              <div class="detail-row" v-if="selectedBooking.contactNotes">
                <p><strong>房客備註:</strong></p>
                <p class="notes-text">{{ selectedBooking.contactNotes }}</p>
              </div>
            </div>

            <hr>

            <div class="detail-section">
              <h6>💳 價格與款項</h6>
              <div class="detail-row">
                <p><strong>總預訂金額:</strong></p>
                <p class="price-value">TWD {{ selectedBooking.totalPrice.toLocaleString() }}</p>
              </div>
              <div class="detail-row">
                <p><strong>房源佣金/淨收入:</strong></p>
                <p class="discount-value">
                   TWD {{ (selectedBooking.totalPrice * 0.9).toLocaleString() }}
                   <span class="text-muted">(10% 佣金)</span>
                </p>
              </div>
              <div class="detail-row">
                <p><strong>付款方式:</strong></p>
                <p>{{ selectedBooking.paymentTiming === 'full' ? '全額預付' : '延後支付' }}</p>
              </div>
              <div class="detail-row warning" v-if="selectedBooking.paymentStatus === 'deferred'">
                <p><strong>付款截止日:</strong></p>
                <p>{{ formatDate(selectedBooking.paymentDeadline, true) }}</p>
              </div>
            </div>

          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">關閉</button>
            <button type="button" class="btn btn-chat">聯絡房客</button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" scoped>
// 樣式調整，以配合房東介面的重點變化
$primary-color: #222;
$secondary-color: #f7a800; // 房東介面常用強調色 (例如橘色/黃色)
$chat-color: #008489; // 聊天/溝通強調色
$border-color: #ebebeb;
$background-light: #f9f9f9;

// 修正 1: 確保所有屬性都在選擇器 { } 內
.host-bookings-page {
  padding: 40px 20px;
  background-color: $background-light;
}

.container {
  max-width: 900px;
  margin: 0 auto;
}

h1 {
  margin-bottom: 30px;
  font-size: 28px;
  color: $primary-color;
}

// 載入和無訂單提示
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
  border-radius: 12px;
  border: 1px solid $border-color;

  p {
    font-size: 18px;
    color: #717171;
    margin-bottom: 20px;
  }

  // 房東主要按鈕顏色
  .btn-primary {
    padding: 10px 20px;
    background-color: $secondary-color;
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    font-weight: bold;

    &:hover {
      background-color: darken($secondary-color, 10%);
    }
  }
}

.bookings-list {
  display: grid;
  gap: 24px;

  .booking-card {
    background: white;
    border: 1px solid $border-color;
    border-radius: 15px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
    overflow: hidden;
    transition: transform 0.2s;

    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      padding: 20px;
      border-bottom: 1px solid $border-color;
      gap: 8px;

      h3 {
        font-size: 20px;
        font-weight: 600;
      }
    }

    .card-body {
      padding: 20px;

      .order-info-grid {
        display: grid; // 補上 display: grid; 確保網格佈局生效
        grid-template-columns: 100px 1fr;
        gap: 12px 10px;

        .highlight-info {
          font-weight: bold;
          color: $secondary-color; // 強調房客姓名
          font-size: 16px;
        }

        .total-price strong {
          color: $primary-color;
        }

        .total-price-value strong {
          color: $primary-color;
        }
      }
    }

    .card-footer {
      padding: 15px 20px;
      text-align: right;
      border-top: 1px solid $border-color;
    }
  }
}

// 通用按鈕樣式
.btn-details {
  background-color: $secondary-color;
  color: white;
  border-color: $secondary-color;
  padding: 10px 18px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;

  &:hover {
    background-color: darken($secondary-color, 10%);
    border-color: darken($secondary-color, 10%);
  }
}

// 狀態標籤 (保持一致性)
.order-status {
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 600;
  display: inline-block;
}

.status-deferred {
  background-color: #fff3cd;
  color: #856404;
}

.status-completed {
  background-color: #d4edda;
  color: #155724;
}

.status-cancelled {
  background-color: #f8d7da;
  color: #721c24;
}

// ==================== Modal 樣式區塊 ====================

.modal-content {
  border-radius: 15px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
}

.detail-section {
  h6 {
    margin-bottom: 15px;
    font-size: 16px;
    font-weight: 600;
  }
}

.detail-section.highlight {
  background-color: #fffde7; // 淺黃色高亮背景
  padding: 15px;
  border-radius: 10px;
  border: 1px solid #ffecb3;

  h6 {
    border-bottom: 1px solid #ffecb3;
    padding-bottom: 8px;
  }
}

.detail-row {
  display: grid;
  grid-template-columns: 120px 1fr;
  margin-bottom: 8px;
  font-size: 14px;
  align-items: center;

  p {
    margin-bottom: 0;
  }

  .text-large {
    font-size: 16px;
    font-weight: 700;
    color: $primary-color;
  }

  .discount-value {
    color: #155724; // 淨收入使用綠色
    font-weight: 600;
  }

  .text-muted {
    font-weight: normal;
    color: #717171 !important;
    font-size: 13px;
  }
  .notes-text {
    font-style: italic;
    color: #717171;
  }
}

// Modal Footer 新增聊天按鈕
.btn-chat {
  background-color: $chat-color;
  color: white;
  border: 1px solid $chat-color;
  padding: 8px 20px;
  border-radius: 8px;
  font-weight: 600;
  transition: all 0.2s;

  &:hover {
    background-color: darken($chat-color, 10%);
    border-color: darken($chat-color, 10%);
  }
}
</style>
