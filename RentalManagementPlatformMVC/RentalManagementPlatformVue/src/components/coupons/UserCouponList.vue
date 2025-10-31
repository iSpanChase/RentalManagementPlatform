<script setup lang="ts">
import { ref, onMounted } from 'vue';
import CouponCard from './CouponCard.vue';
import CouponDetailModal from './CouponDetailModal.vue';
import { useCouponStore } from '@/stores/coupon.js';
import type { Coupon } from '@/types/coupon';

const userId = 1; // 模擬使用者 ID

const couponStore = useCouponStore();

const detailVisible = ref(false);
const selectedCoupon = ref<Coupon | null>(null);

// 元件掛載時從 store 獲取該使用者的優惠券
onMounted(() => {
  couponStore.fetchCoupons(userId);
});

function onRedeem(code: string) {
  alert(`領取成功: ${code}`);
  // 重新獲取優惠券列表以更新狀態
  couponStore.fetchCoupons(userId);
}

function showDetail(coupon: Coupon) {
  selectedCoupon.value = coupon;
  detailVisible.value = true;
}
</script>

<template>
  <div>
    <h1>我的優惠券</h1>
    <div v-if="couponStore.isLoading">載入中...</div>
    <div v-else-if="couponStore.error">{{ couponStore.error }}</div>
    <div v-else class="grid">
      <CouponCard
        v-for="c in couponStore.coupons"
        :key="c.couponId"
        :coupon="c"
        :userId="userId"
        @redeem="onRedeem"
        @show-detail="showDetail"
      />
    </div>

    <CouponDetailModal v-if="detailVisible" :coupon="selectedCoupon" :visible="detailVisible" :userId="userId" @close="detailVisible=false"/>
  </div>
</template>

<style scoped>
.grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(250px,1fr)); gap:16px; }
</style>
