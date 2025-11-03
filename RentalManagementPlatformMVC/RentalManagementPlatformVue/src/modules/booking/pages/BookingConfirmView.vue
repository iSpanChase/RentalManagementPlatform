<script setup>
import { onMounted, computed, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useBookingStore } from '@/stores/bookingStore';
import { useAuthStore } from '@/stores/authStore.js';
import { fetchRoomDetail } from '@/api/roomSearchApi';
import { useToast } from 'vue-toastification';
import BookingPaymentsStepsCard from '../components/BookingPaymentsStepsCard.vue';
import BookingSummaryCard from '../components/BookingSummaryCard.vue';
import PriceSummary from '../components/PriceSummary.vue';

const bookingStore = useBookingStore();
const route = useRoute();
const router = useRouter();
const toast = useToast();
const authStore = useAuthStore();

const isLoading = ref(true);
const isError = ref(false);

// 價格摘要元件引用
const priceSummary = ref(null);

onMounted(async () => {
  isLoading.value = true;
  const roomId = Number(route.query.roomId);

  if (roomId && !isNaN(roomId)) {
    bookingStore.clearBookingDraft();
    try {
      const roomDetail = await fetchRoomDetail(roomId);
      if (roomDetail) {
        // 取得今天日期作為入住日
        const checkInDate = new Date();

        // 退房為隔天上午 11 點
        const checkOutDate = new Date();
        checkOutDate.setDate(checkOutDate.getDate() + 1);

        const bookingData = {
          roomId: roomDetail.roomId,
          guestId: authStore.currentUser?.id || 1, // 暫時使用硬編碼的 userId = 1，直到會員模組完成
          guestCount: 1,
          roomTitle: roomDetail.title,
          roomImage: roomDetail.mainImageUrl || (roomDetail.photoUrls && roomDetail.photoUrls[0]) || '',
          pricePerNight: roomDetail.pricePerNight,
          checkIn: checkInDate.toISOString(),
          checkOut: checkOutDate.toISOString()
        };
        bookingStore.setBookingDraft(bookingData);
        toast.success('房源資料已載入!');
      } else {
        throw new Error('找不到房源資料');
      }
    } catch (error) {
      toast.error(error.message || '載入房源資料失敗');
      isError.value = true;
    } finally {
      isLoading.value = false;
    }
  }
  else {
    if (!bookingStore.hasBookingDraft) {
      toast.error('訂房資料不存在，請重新選擇房源');
      isError.value = true;
      router.push({ name: 'home' });
    }
    isLoading.value = false;
  }
});


// 取得最終價格的計算值
const finalPrice = computed(() => priceSummary.value?.finalPrice || bookingStore.totalPrice);
</script>

<template>
  <div class="booking-confirm-page">
    <h1>確認預定</h1>

    <!-- Loading -->
    <div v-if="isLoading || bookingStore.isLoading" class="loading-overlay">
      <div class="loading-content">
        <div class="loading-spinner"></div>
        <p>{{ bookingStore.isLoading ? '正在處理您的訂單...' : '正在載入房源資訊...' }}</p>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="isError" class="error-message-container">
      <div class="error-card">
        <h2>無法載入頁面</h2>
        <p>抱歉，載入房源資訊時發生錯誤，或該房源不存在。</p>
        <button @click="router.push({ name: 'home' })" class="btn-back-home">返回首頁</button>
      </div>
    </div>

    <!-- Content -->
    <div v-else-if="bookingStore.hasBookingDraft" class="container">
      <!-- 左側:付款步驟 -->
      <BookingPaymentsStepsCard :total-price="finalPrice" />

      <!-- 右側:預訂摘要 -->
      <BookingSummaryCard>
        <template #price-summary>
          <PriceSummary ref="priceSummary" />
        </template>
      </BookingSummaryCard>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.booking-confirm-page {
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
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: 40px;
  align-items: start;
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  justify-content: center;

  @media (max-width: 1024px) {
    gap: 24px;
    grid-template-columns: 550px 380px;
  }

  @media (max-width: 992px) {
    grid-template-columns: 500px 350px;
    gap: 20px;
  }

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    gap: 24px;
  }
}

/* Loading & Error Styles */
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
  padding: 20px;
}

.loading-content, .error-card {
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

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>
