<script setup>
import { useBookingStore } from '@/stores/bookingStore'
import { useRouter } from 'vue-router'
import { computed } from 'vue' 
import BookingPaymentsStepsCard from '../components/BookingPaymentsStepsCard.vue'
import BookingSummaryCard from '../components/BookingSummaryCard.vue'
import CouponSelector from '@/components/coupons/CouponSelector.vue' 
import { useCouponCalculator } from '@/composables/useCouponCalculator.js'

const bookingStore = useBookingStore()
const router = useRouter()
const goHome = () => {
  router.push('/')
}

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

    <!-- Loading 遮罩 -->
    <div v-if="bookingStore.isLoading" class="loading-overlay">
      <div class="loading-content">
        <div class="loading-spinner"></div>
        <p>正在處理您的訂單...</p>
      </div>
    </div>

    <div class="container">
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

/* Loading 遮罩樣式 */
.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.loading-content {
  background: white;
  padding: 40px;
  border-radius: 16px;
  text-align: center;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  max-width: 300px;
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
