<script setup>
import { onMounted, computed, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useBookingStore } from '@/stores/bookingStore';
import { useAuthStore } from '@/stores/authStore.js';
import { fetchRoomDetail } from '@/api/roomSearchApi';
import { useToast } from 'vue-toastification';
import BookingPaymentsStepsCard from '../components/BookingPaymentsStepsCard.vue';
import BookingSummaryCard from '../components/BookingSummaryCard.vue';
import CouponSelector from '@/components/coupons/CouponSelector.vue' 
import { useCouponCalculator } from '@/composables/useCouponCalculator.js'

const bookingStore = useBookingStore();
const route = useRoute();
const router = useRouter();
const toast = useToast();
const authStore = useAuthStore();

const isLoading = ref(true);
const isError = ref(false);

onMounted(async () => {
  isLoading.value = true;
  const roomId = Number(route.query.roomId);

  // Case 1: Rebook flow (roomId is present in query)
  if (roomId && !isNaN(roomId)) {
    bookingStore.clearBookingDraft(); // Clear previous draft for rebooking
    try {
      const roomDetail = await fetchRoomDetail(roomId);
      if (roomDetail) {
        const bookingData = {
          roomId: roomDetail.roomId,
          guestId: authStore.currentUser?.id || 1, // 暫時使用硬編碼的 userId = 1，直到會員模組完成
          guestCount: 1,
          roomTitle: roomDetail.title,
          roomImage: roomDetail.mainImageUrl || (roomDetail.photoUrls && roomDetail.photoUrls[0]) || '',
          pricePerNight: roomDetail.pricePerNight,
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
  // Case 2: Normal booking flow (coming from RoomDetailView)
  else {
    if (!bookingStore.hasBookingDraft) {
      toast.error('訂房資料不存在，請重新選擇房源');
      isError.value = true;
      router.push({ name: 'home' });
    }
    isLoading.value = false;
  }
});


// For Coupon Calculator
const cartInfo = computed(() => ({
  totalAmount: bookingStore.subtotal, // 使用小計作為計算基礎
  leaseDays: bookingStore.nights,
  userId: bookingStore.bookingDraft?.guestId,
  // 這邊可以根據需要從 bookingStore 填充更多資訊
  // cityId: bookingStore.bookingDraft?.cityId,
  useDate: new Date(bookingStore.bookingDraft?.checkIn)
}));

const {
  couponOptions,
  selectedCouponId,
  discountAmount,
  finalPrice,
} = useCouponCalculator(cartInfo);
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
      <BookingSummaryCard :hide-internal-total="true">
        <template #coupon>
          <div class="coupon-section">
            <CouponSelector
              :coupons="couponOptions"
              v-model="selectedCouponId"
            />
            <div v-if="discountAmount > 0" class="price-row discount">
              <span>優惠券折扣</span>
              <span class="green">-${{ discountAmount.toLocaleString() }} TWD</span>
            </div>
          </div>

          <hr v-if="discountAmount > 0">

          <div class="price-row total">
            <strong>最終總計 TWD</strong>
            <strong>${{ finalPrice.toLocaleString() }} TWD</strong>
          </div>
        </template>

        <template #price-details-body>
          <div class="price-row">
            <span>{{ bookingStore.nights }} 晚 x ${{ bookingStore.bookingDraft.pricePerNight.toLocaleString() }} TWD</span>
            <span>${{ bookingStore.subtotal.toLocaleString() }} TWD</span>
          </div>

          <div class="price-row discount" v-if="bookingStore.discountAmount > 0">
          </div>

          <div class="price-row discount" v-if="discountAmount > 0">
            <span>優惠券折扣</span>
            <span class="green">-${{ discountAmount.toLocaleString() }} TWD</span>
          </div>

          <hr>

          <div class="price-row total">
            <strong>總計 TWD</strong>
            <strong>${{ finalPrice.toLocaleString() }} TWD</strong>
          </div>
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

// --- 優惠券區塊樣式 (優化版) ---
.coupon-section {
  padding-top: 8px;
  padding-bottom: 8px;

  // 使用 :deep() 穿透 Scoped CSS 來影響子元件
  :deep(.coupon-selector) {
    label {
      display: block;
      margin-bottom: 8px;
      font-weight: 600;
      color: #222;
      font-size: 16px;
    }
  }
}

.price-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
  font-size: 16px;

  &.discount span.green {
    color: #008489;
    font-weight: 600;
  }

  &.total {
    font-size: 16px;
    font-weight: bold;
  }
}
</style>
