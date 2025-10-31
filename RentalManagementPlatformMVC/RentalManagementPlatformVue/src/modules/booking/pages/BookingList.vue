<script setup>
import { ref, onMounted } from 'vue'
import { getBookingList } from '../api/bookingApi'

// 訂單資料
const bookings = ref([])

// 頁面載入時取得訂單
onMounted(async () => {
  const data = await getBookingList()
  bookings.value = data
  console.log('訂單資料:', data)  // 可以在瀏覽器看到資料
})
</script>

<template>
  <div class="booking-list-page">
    <h1>我的訂單</h1>

    <!-- 顯示訂單列表 -->
    <div v-for="booking in bookings" :key="booking.bookingId">
      <p>訂單編號: {{ booking.orderNumber }}</p>
      <p>房間: {{ booking.room }}</p>
      <p>入住日期: {{ booking.checkIn }}</p>
      <p>退房日期: {{ booking.checkOut }}</p>
      <p>總價: ${{ booking.totalPrice }}</p>
      <p>狀態: {{ booking.status }}</p>
      <hr>
    </div>
  </div>
</template>

<style scoped>
.booking-list-page {
  padding: 20px;
}

hr {
  margin: 20px 0;
}
</style>
