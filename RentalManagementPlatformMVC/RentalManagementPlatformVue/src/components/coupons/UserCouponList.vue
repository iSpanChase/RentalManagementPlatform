<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import CouponCard from './CouponCard.vue';
import CouponDetailModal from './CouponDetailModal.vue';
import { useCouponStore } from '@/stores/couponStore.js';
import { useAuthStore } from '@/stores/auth';
import type { Coupon } from '@/types/coupon';

const authStore = useAuthStore();
const userId = computed(() => authStore.state.profile?.userId);

const couponStore = useCouponStore();

const detailVisible = ref(false);
const selectedCoupon = ref<Coupon | null>(null);

// 監聽 userId，並在獲取到後加載優惠券
watch(userId, (newUserId) => {
  if (newUserId) {
    couponStore.fetchUserCoupons(newUserId);
  }
}, { immediate: true });


function onRedeem(code: string) {
  alert(`領取成功: ${code}`);
  // 重新獲取優惠券列表以更新狀態
  if (userId.value) {
    couponStore.fetchUserCoupons(userId.value);
  }
}

function showDetail(coupon: Coupon) {
  selectedCoupon.value = coupon;
  detailVisible.value = true;
}
</script>

<template>
  <div>
    <h1>我的優惠券</h1>
    <div v-if="couponStore.loading">載入中...</div>
    <div v-else-if="couponStore.error">{{ couponStore.error }}</div>
    <div v-else class="grid">
      <CouponCard
        v-for="c in couponStore.userCoupons"
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
