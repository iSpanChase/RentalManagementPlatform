<!-- 優惠券列表首頁 -->

<template>
  <div class="coupons-page">
    <h1>探索您的專屬優惠</h1>
    <div v-if="isLoading">載入中...</div>
    <div v-else-if="isError">{{ error?.message }}</div>
    <div v-else class="coupons-grid">
      <CouponCard
        v-for="coupon in coupons"
        :key="coupon.couponId"
        :coupon="coupon"
        :userId="userId"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useQuery } from '@tanstack/vue-query'
import { getPublicCoupons } from '../services/CouponService.ts'
import CouponCard from '../components/coupons/CouponCard.vue'
import { onMounted, computed } from 'vue';
import { useAuthStore } from '@/stores/auth';

console.log('CouponView component is setting up...');

onMounted(() => {
  console.log('CouponView component has mounted.');
});

const authStore = useAuthStore();
const userId = computed(() => authStore.state.profile?.userId);

const { data: coupons, isLoading, isError, error } = useQuery({
  queryKey: ['coupons'],
  queryFn: () => {
    console.log('Fetching coupons...');
    return getPublicCoupons();
  },
  select: (data) => {
    console.log('Coupons data received:', data);
    if (Array.isArray(data)) {
      return data.map(c => ({ ...c, status: 'available' }))
    }
    return [];
  }
})
</script>

<style scoped>
.coupons-page { max-width:1200px; margin:0 auto; padding:20px; }
.coupons-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(280px,1fr)); gap:24px; }
</style>
