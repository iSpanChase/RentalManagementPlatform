<script setup>
import { ref, onMounted } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';

const bookingStore = useBookingStore();
const bookings = ref([]);
const isLoading = ref(false);

onMounted(async () => {
  isLoading.value = true;
  try {
    // 呼叫 store action 來獲取 ID 為 1 的使用者的訂單 (開發用)
    const userId = 1;
    bookings.value = await bookingStore.fetchUserBookings(userId);
    console.log('從後端獲取訂單成功:', bookings.value);
  } catch (error) {
    console.error('獲取訂單失敗:', error);
    alert('無法載入訂單資料');
  } finally {
    isLoading.value = false;
  }
});

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
        form.submit();
      } else {
        throw new Error('在回傳的 HTML 中找不到 form 標籤');
      }
    } else {
      throw new Error(result.message || '獲取付款表單失敗');
    }
  } catch (error) {
    console.error(`為訂單 ${orderNumber} 付款失敗:`, error);
    alert(error.message || '準備付款時發生錯誤，請稍後再試');
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="my-bookings-page">
    <div class="container">
      <h1>我的訂單</h1>
      <div v-if="isLoading" class="loading-spinner"></div>
      <div v-else-if="bookings.length === 0" class="no-bookings">
        <p>您目前沒有任何訂單。</p>
      </div>
      <div v-else class="bookings-list">
        <div v-for="booking in bookings" :key="booking.orderNumber" class="booking-card">
          <div class="card-header">
            <h3>{{ booking.roomTitle }}</h3>
            <span :class="`status-${booking.paymentStatus}`">{{ booking.paymentStatus }}</span>
          </div>
          <div class="card-body">
            <p><strong>訂單編號:</strong> {{ booking.orderNumber }}</p>
            <p><strong>入住日期:</strong> {{ booking.checkIn }}</p>
            <p><strong>退房日期:</strong> {{ booking.checkOut }}</p>
            <p><strong>總金額:</strong> TWD {{ booking.totalPrice.toLocaleString() }}</p>
          </div>
          <div class="card-footer">
            <button v-if="booking.paymentStatus === 'deferred'" @click="handlePayNow(booking.orderNumber)" class="btn-pay-now">
              立即付款
            </button>
            <button class="btn-details">查看詳情</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.my-bookings-page {
  padding: 40px 20px;
  background-color: #f9f9f9;
}
.container {
  max-width: 900px;
  margin: 0 auto;
}
h1 {
  margin-bottom: 30px;
}
.bookings-list {
  display: grid;
  gap: 20px;
}
.booking-card {
  background: white;
  border: 1px solid #eee;
  border-radius: 12px;
  box-shadow: 0 4px 15px rgba(0,0,0,0.05);
  overflow: hidden;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px 20px;
  background-color: #f5f5f5;
  border-bottom: 1px solid #eee;
}
.card-header h3 {
  margin: 0;
  font-size: 18px;
}
.card-body {
  padding: 20px;
  display: grid;
  gap: 10px;
}
.card-body p {
  margin: 0;
}
.card-footer {
  padding: 15px 20px;
  text-align: right;
  border-top: 1px solid #eee;
}
.btn-pay-now, .btn-details {
  padding: 8px 16px;
  border-radius: 6px;
  border: 1px solid #ccc;
  background-color: #fff;
  cursor: pointer;
  margin-left: 10px;
}
.btn-pay-now {
  background-color: #222;
  color: white;
  border-color: #222;
}
.status-deferred {
  background-color: #fff3cd;
  color: #856404;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: bold;
}
.status-completed {
  background-color: #d4edda;
  color: #155724;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: bold;
}
</style>
