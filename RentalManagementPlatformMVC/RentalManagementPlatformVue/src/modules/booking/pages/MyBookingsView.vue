<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useRoute, useRouter } from 'vue-router';
import { Modal } from 'bootstrap';

const bookingStore = useBookingStore();
const router = useRouter(); // 取得 router 實例
const bookings = ref([]);
const isLoading = ref(false);
const route = useRoute();
const testUserId = 1;
const selectedBooking = ref(null);
const cancelModal = ref(null);
const bookingToCancel = ref(null);

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

const handleContactHost = (booking) => {
  console.log('準備聯繫房東，訂單:', booking.orderNumber, '房源:', booking.room);
  alert(`聯繫房東功能開發中... (房源: ${booking.room})`);
};

const openCancelConfirmModal = (booking) => {
  bookingToCancel.value = booking;
  if (cancelModal.value) {
    cancelModal.value.show();
  }
};

const confirmCancellation = async () => {
  if (!bookingToCancel.value) return;

  const bookingId = bookingToCancel.value.bookingId;
  const orderNumber = bookingToCancel.value.orderNumber;
  console.log(`確認取消訂單 (ID: ${bookingId})`);
  isLoading.value = true;

  try {
    // 1. 呼叫 store action，預期會收到一個包含 success 和 booking 的物件
    const response = await bookingStore.cancelBooking(bookingId);

    // 2. 檢查回應是否成功，且包含 booking 物件
    if (response && response.success && response.booking) {
      alert(response.message || '訂單已成功取消');
      const updatedBooking = response.booking; // 提取真正的訂單物件

      // 3. 使用回傳的 booking 物件來更新 bookings 陣列
      bookings.value = bookings.value.map(booking =>
        booking.bookingId === updatedBooking.bookingId ? updatedBooking : booking
      );
    } else {
      // 處理後端回傳 success: false 或資料結構不符的狀況
      alert(response?.message || '取消訂單失敗，請稍後再試');
    }
  } catch (error) {
    // axios 捕捉到 400/404/500 錯誤
    const errorMessage = error.response?.data?.message || error.message || '取消訂單時發生錯誤';
    alert(errorMessage);
  } finally {
    isLoading.value = false;
    if (cancelModal.value) {
      cancelModal.value.hide(); // 關閉彈窗
    }
    bookingToCancel.value = null; // 清除
  }
};

const handleRebook = (booking) => {
  console.log('準備重新預訂，房源:', booking.room);
  alert(`重新預訂功能開發中... (房源: ${booking.room})`);
};

onMounted(async () => {
  isLoading.value = true;

  // --- 監聽詳情 Modal ---
  const modalElement = document.getElementById('orderDetailModal');
  if (modalElement) {
    modalElement.addEventListener('hidden.bs.modal', handleModalHidden);
  }

  // --- 初始化取消 Modal ---
  const cancelModalElement = document.getElementById('cancelConfirmModal');
  if (cancelModalElement) {
    cancelModal.value = new Modal(cancelModalElement);
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
                  @click="handleContactHost(booking)"
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
                  @click="handleRebook(booking)"
                  v-if="booking.paymentStatus === 'cancelled' || booking.paymentStatus === 'completed'"
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
              <span v-if="isLoading" class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
              <span v-else>確認取消</span>
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
$secondary-color: #008489;     // 主要強調色（綠）
$danger-color: #d9534f;        // 取消/危險色（紅）
$border-color: #ebebeb;
$background-light: #f9f9f9;
$text-light: #717171;
$text-dark: #484848;

// ==================== 頁面基礎 ====================
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

// 載入中
.loading-spinner {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 200px;
}

// 無訂單提示
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

// ==================== 訂單卡片列表 ====================
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

    // 圖片區
    .card-image-wrapper {
      width: 220px;
      flex-shrink: 0;

      .room-image {
        width: 100%;
        height: 100%;
        object-fit: cover;
      }
    }

    // 內容區
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

    // 頂部：房源資訊
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

    // 中間：訂單細節
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

    // 底部：價格 + 按鈕群組
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

// ==================== 按鈕群組（核心更新）================
// 基礎按鈕樣式
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

// 詳情按鈕
.btn-details,
.btn-rebook {
  background-color: $background-light;
  color: $primary-color;
  border-color: #ddd;

  &:hover {
    background-color: $primary-color;
    color: white;
    border-color: $primary-color;
  }
}

// 付款按鈕
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

// 聯繫房東按鈕
.btn-contact {
  background-color: white;
  color: $secondary-color;
  border-color: $secondary-color;

  &:hover {
    background-color: $secondary-color;
    color: white;
  }
}

// 取消預訂按鈕
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

// ==================== 狀態標籤 ====================
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

// ==================== Modal 樣式 ====================
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

  .discount-value {
    color: #e63946;
    font-weight: 500;
  }

  &.warning p:last-child {
    color: #e63946;
    font-weight: 600;
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
    background-color: darken($text-light, 10%);
    border-color: darken($text-light, 10%);
  }
}

// ==================== 響應式設計 (RWD) ====================
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

// ==================== 取消彈窗按鈕 ====================
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
