<template>
  <div class="container mt-4">
    <h2 class="mb-4">可領取優惠券</h2>

    <!-- 載入中狀態 -->
    <div v-if="couponStore.isLoading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">載入中...</span>
      </div>
      <p class="mt-2">優惠券載入中...</p>
    </div>

    <!-- 錯誤狀態 -->
    <div v-else-if="couponStore.error" class="alert alert-danger" role="alert">
      <h4 class="alert-heading">載入失敗！</h4>
      <p>{{ couponStore.error }}</p>
      <hr>
      <p class="mb-0">請稍後再試，或聯繫客服。</p>
    </div>

    <!-- 空狀態 -->
    <div v-else-if="couponStore.coupons.length === 0 && !couponStore.isLoading" class="text-center py-5">
      <img src="https://via.placeholder.com/150/007bff/ffffff?text=No+Coupons" alt="No Coupons" class="mb-3">
      <h3>目前沒有可領取的優惠券</h3>
      <p class="text-muted">敬請期待，新的優惠券將會陸續上架！</p>
      <button class="btn btn-outline-primary mt-3" @click="couponStore.fetchCoupons()">重新整理</button>
    </div>

    <!-- 優惠券列表 -->
    <div v-else class="row">
      <div class="col-md-6 col-lg-4 mb-4" v-for="coupon in couponStore.coupons" :key="coupon.couponId">
        <CouponCard :coupon="coupon" />
      </div>
    </div>


  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useCouponStore } from '../stores/coupon.js';
import CouponCard from '../components/coupons/CouponCard.vue';

const couponStore = useCouponStore();

console.log('View - couponStore.coupons:', couponStore.coupons); // <-- 加入這行

onMounted(() => {
  couponStore.fetchCoupons(); // 元件掛載時獲取優惠券列表
});
</script>

<style scoped>
/* 您可以在這裡添加 CouponListView 特有的樣式 */
.container {
  max-width: 960px;
}
</style>
