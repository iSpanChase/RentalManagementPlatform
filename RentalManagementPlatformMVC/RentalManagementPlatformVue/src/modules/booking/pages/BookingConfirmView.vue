<script setup>
import { useBookingStore } from '@/stores/bookingStore'
import { useRouter } from 'vue-router'
import BookingPaymentsStepsCard from '../components/BookingPaymentsStepsCard.vue'
import BookingSummaryCard from '../components/BookingSummaryCard.vue'

const bookingStore = useBookingStore()
const router = useRouter()

if (!bookingStore.hasBookingDraft) {
  console.log('沒有訂房資料，載入假資料...')
  bookingStore.setMockData()
}

const goHome = () => {
  router.push('/')
}
</script>

<template>
  <div class="booking-confirm-page">
    <h1>確認預定</h1>

    <div class="container">
      <!-- 左側:付款步驟 -->
      <BookingPaymentsStepsCard :total-price="bookingStore.totalPrice" />

      <!-- 右側:預訂摘要 -->
      <BookingSummaryCard />
    </div>
  </div>
</template>

<style lang="scss" scoped>
.booking-confirm-page {
  max-width: 1024px;
  margin: 0 auto;
  padding: 10px 20px;
  h1 {
    font-size: 28px;
    font-weight: bold;
    margin-bottom: 24px;
  }
}

.container {
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: 80px;
}

@media (max-width: 768px) {
  .container {
    grid-template-columns: 1fr;
  }
}
</style>
