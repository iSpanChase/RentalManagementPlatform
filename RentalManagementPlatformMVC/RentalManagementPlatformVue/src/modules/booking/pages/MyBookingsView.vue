<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useRoute } from 'vue-router';

const bookingStore = useBookingStore();
const bookings = ref([]);
const isLoading = ref(false);
const route = useRoute();
const testUserId = 1;
const selectedBooking = ref(null);

// ==================== 工具函數 ====================
/**
 * 格式化日期
 * @param {string} dateStr - ISO 日期字串
 * @returns {string} 格式化後的日期
 */
const formatDate = (dateStr) => {
  const date = new Date(dateStr);
  return date.toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};

// 取得狀態文字 (包含新欄位)
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
  // 1. 找到對應的完整訂單數據
  const booking = bookings.value.find(b => b.orderNumber === orderNumber);
  if (!booking) {
    console.error(`找不到訂單 ${orderNumber}`);
    return;
  }

  // 2. 設置選定的訂單，這會觸發 Modal 內部的 v-if
  selectedBooking.value = booking;

  // 3. 移除手動 modal.show() 邏輯
  console.log(`設置訂單 ${orderNumber} 資料，準備透過 data-bs-toggle 開啟 Modal`);
};

// 處理 Modal 隱藏事件
const handleModalHidden = () => {
    console.log('Modal 已隱藏，清除 selectedBooking 資料');
    selectedBooking.value = null;
};

// 處理立即付款
const handlePayNow = async (orderNumber) => {
  console.log(`準備為訂單 ${orderNumber} 付款`);
  isLoading.value = true;
  try {
    const result = await bookingStore.getDeferredPaymentForm(orderNumber);

    if (result.success && result.ecpayFormHtml) {
      console.log('成功獲取付款表單，準備跳轉...');
      const tempDiv = document.createElement('div');
      tempDiv.innerHTML = result.ecpayFormHtml;
      document.body.appendChild(tempDiv);

      const form = tempDiv.querySelector('form');
      if (form) {
        setTimeout(() => {
          form.submit();
        }, 500);
      } else {
        alert('無法找到付款表單，請聯繫客服');
      }
    } else {
      alert(result.message || '獲取付款表單失敗');
    }
  } catch (error) {
    console.error(`為訂單 ${orderNumber} 付款失敗:`, error);
    alert(error.message || '準備付款時發生錯誤，請稍後再試');
  } finally {
    isLoading.value = false;
  }
};

onMounted(async () => {
  isLoading.value = true;

  // ==================== Modal 事件監聽設置 ====================
  const modalElement = document.getElementById('orderDetailModal');
  if (modalElement) {
    // 監聽 Bootstrap 的 Modal 完全隱藏後的事件
    modalElement.addEventListener('hidden.bs.modal', handleModalHidden);
  }

  try {
    let userId = route.query.userId;
    if (!userId) {
      console.warn('URL 中未提供 userId，使用測試用戶 ID');
      userId = testUserId;
    }

    const fetchedBookings = await bookingStore.fetchUserBookings(userId);

    bookings.value = fetchedBookings.map(b => ({...b}));

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
  <div class="my-bookings-page">
    <div class="container">
      <h1>我的預訂</h1>

      <div v-if="isLoading" class="loading-spinner">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">載入中...</span>
        </div>
      </div>

      <div v-else-if="bookings.length === 0" class="no-bookings">
        <p>您目前沒有任何預訂。</p>
        <router-link to="/">
          <button class="btn-primary">開始探索房源</button>
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
              <p><strong>訂單編號:</strong></p>
              <p>{{ booking.orderNumber }}</p>

              <p><strong>入住日期:</strong></p>
              <p>{{ formatDate(booking.checkIn) }}</p>

              <p><strong>退房日期:</strong></p>
              <p>{{ formatDate(booking.checkOut) }}</p>

              <p class="total-price"><strong>總金額:</strong></p>
              <p class="total-price-value">
                <strong>${{ booking.totalPrice.toLocaleString() }} TWD</strong>
              </p>
            </div>
          </div>
          <div class="card-footer">
            <button
              v-if="booking.paymentStatus === 'deferred'"
              @click="handlePayNow(booking.orderNumber)"
              class="btn-pay-now"
              :disabled="isLoading"
            >
              立即付款
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

  <Teleport to="body">
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
                <p>{{ formatDate(selectedBooking.createdAt, true) }}</p>
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
              <div class="detail-row" v-if="selectedBooking.pointsRedeemed > 0">
                <p><strong>點數折抵:</strong></p>
                <p class="discount-value">- {{ selectedBooking.pointsRedeemed.toLocaleString() }} 點</p>
              </div>
              <div class="detail-row">
                <p><strong>付款方式:</strong></p>
                <p>{{ selectedBooking.paymentTiming === 'full' ? '全額預付' : '延後支付' }}</p>
              </div>
              <div class="detail-row warning" v-if="selectedBooking.PaymentStatus === 'deferred'">
                <p><strong>付款截止日:</strong></p>
                <p>{{ formatDate(selectedBooking.paymentDeadline, true) }}</p>
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
              <div class="detail-row" v-if="selectedBooking.ContactNotes">
                <p><strong>特殊需求:</strong></p>
                <p class="notes-text">{{ selectedBooking.contactNotes }}</p>
              </div>
            </div>

            <hr>

            <div class="detail-section">
              <h6><i class="fa-solid fa-address-book"></i> 帳單地址</h6>
              <div class="address-block">
                {{ selectedBooking.billingCountry }}
                {{ selectedBooking.billingZipCode }}
                {{ selectedBooking.billingCity }}
                {{ selectedBooking.billingState }}
                {{ selectedBooking.billingStreet }}
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
  </Teleport>
</template>

<style lang="scss" scoped>
$primary-color: #222;
$secondary-color: #008489; // 綠色強調色
$border-color: #ebebeb;
$background-light: #f9f9f9;

.my-bookings-page {
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
  gap: 24px; // 增加卡片間距

  .booking-card {
    background: white;
    border: 1px solid $border-color;
    border-radius: 15px; // 增大圓角
    box-shadow: 0 4px 15px rgba(0,0,0,0.08); // 輕微陰影
    overflow: hidden;
    transition: transform 0.2s;

    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(0,0,0,0.1);
    }

    .card-header {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      padding: 20px;
      background-color: white; // 移除灰色背景，保持簡潔
      border-bottom: 1px solid $border-color;
      gap: 8px;

      h3 {
        margin: 0;
        font-size: 20px;
        font-weight: 600;
        color: $primary-color;
      }
    }

    .card-body {
      padding: 20px;

      .order-info-grid {
        display: grid;
        grid-template-columns: 100px 1fr; // 標籤和值分兩欄
        gap: 12px 10px; // 行間距和列間距

        p {
          margin: 0;
          font-size: 15px;

          & strong {
            color: #717171; // 標籤顏色
            font-weight: normal;
          }
        }

        // 特別強調總金額
        .total-price {
            grid-column: 1 / 2;
            & strong {
                font-weight: bold;
                color: $primary-color;
            }
        }
        .total-price-value {
            grid-column: 2 / 3;
            text-align: right;
            strong {
                font-weight: 700;
                color: $primary-color;
            }
        }
      }
    }

    .card-footer {
      padding: 15px 20px;
      text-align: right;
      border-top: 1px solid $border-color;

      // 按鈕間距調整
      & > button {
        margin-left: 10px;
      }
    }
  }
}

// 通用按鈕樣式
.btn-pay-now,
.btn-details {
  padding: 10px 18px;
  border-radius: 8px; // 增大圓角
  border: 1px solid #ccc;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s;
}

.btn-details {
  background-color: white;
  color: $primary-color;
  border-color: $primary-color;

  &:hover {
    background-color: $primary-color;
    color: white;
  }
}

.btn-pay-now {
  background-color: $secondary-color; // 使用強調色
  color: white;
  border-color: $secondary-color;

  &:hover {
    background-color: darken($secondary-color, 10%);
    border-color: darken($secondary-color, 10%);
  }

  &:disabled {
    background-color: #ccc;
    border-color: #ccc;
    cursor: not-allowed;
    color: #717171;
  }
}

// 狀態標籤
.order-status {
  padding: 4px 10px;
  border-radius: 20px; // 更圓的標籤
  font-size: 13px;
  font-weight: 600;
  display: inline-block;
}

.status-deferred { // 待付款 (警告色)
  background-color: #fff3cd;
  color: #856404;
}

.status-completed { // 已完成 (成功色)
  background-color: #d4edda;
  color: #155724;
}

.status-cancelled { // 已取消 (一般色)
  background-color: #f8d7da;
  color: #721c24;
}

// ==================== Modal 樣式區塊 ====================

.modal-content {
  border-radius: 15px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.15);

  .modal-header {
    border-bottom: 1px solid #ebebeb;
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
  }
}

.detail-row {
  display: grid;
  grid-template-columns: 120px 1fr; // 標籤和值分兩欄
  margin-bottom: 8px;
  font-size: 14px;
  align-items: center;

  p {
    margin: 0;
    line-height: 1.5;
  }

  strong {
    color: #717171;
    font-weight: 500;
  }

  .price-value {
    font-weight: 700;
    color: $secondary-color;
    font-size: 16px;
  }

  .discount-value {
    color: #e63946; // 紅色表示扣除
    font-weight: 500;
  }

  &.warning {
    p:last-child {
        color: #e63946;
        font-weight: 600;
    }
  }
}

.notes-text {
  background-color: #f7f7f7;
  padding: 10px;
  border-radius: 8px;
  color: $primary-color;
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
  border-color: #ebebeb;
  margin: 20px 0;
}

.modal-footer {
    border-top: none;
    padding: 15px 25px;
}

// 覆寫按鈕樣式
.btn-secondary {
    background-color: #717171;
    border-color: #717171;
    color: white;
    padding: 8px 20px;
    border-radius: 8px;
    &:hover {
        background-color: darken(#717171, 10%);
        border-color: darken(#717171, 10%);
    }
}
</style>
