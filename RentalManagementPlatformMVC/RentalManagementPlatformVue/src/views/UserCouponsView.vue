<!-- 使用者已領取優惠清單 -->
<template>  
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">我的優惠券</h1>

    <div v-if="isLoading" class="text-gray-500">載入中...</div>
    <div v-else-if="isError" class="text-red-500">{{ error?.message }}</div>

    <div v-else-if="userCoupons && userCoupons.length === 0" class="text-gray-400">
      尚未領取任何優惠券。
    </div>

    <div v-else-if="userCoupons" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <CouponCard
        v-for="coupon in userCoupons"
        :key="coupon.couponId"
        :coupon="coupon"
        :userId="userId"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useQuery } from '@tanstack/vue-query'
import CouponCard from '@/components/coupons/CouponCard.vue'
import { getUserCoupons } from '@/services/CouponService.js'
// 暫時在本地定義 Coupon 型別，以解決模組找不到的問題
interface Coupon {
  couponId: number;
  couponName: string;
  discountCode: string;
  discountMethod: 'Percentage' | 'Amount';
  discountQuota: number;
  lowSpend: number;
  startDate: string;
  endDate: string;
  status: string;
} 

const userId = 1 // 模擬目前登入使用者

const { data: userCoupons, isLoading, isError, error } = useQuery({
  queryKey: ['userCoupons', userId],
  queryFn: () => getUserCoupons(userId),
})
</script>
