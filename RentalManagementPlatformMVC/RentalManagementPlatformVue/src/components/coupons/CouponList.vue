<template>
  <div class="coupon-list">
    <div v-for="coupon in coupons" :key="coupon.couponId" class="flex flex-col">
      <CouponCard 
        :coupon="coupon" 
        :user-id="userId" 
        @gotoUse="handleGotoUse"
        @showDetails="(coupon) => emit('showDetails', coupon)"
      />
    </div>
  </div>
</template>

<script setup>
import CouponCard from './CouponCard.vue';

const props = defineProps({
  coupons: { type: Array, required: true },
  userId: { type: Number, required: true },
});

const emit = defineEmits(['gotoUse', 'showDetails']);

const handleGotoUse = (couponId) => {
  emit('gotoUse', couponId);
};
</script>

<style scoped>
.coupon-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); /* Adjusted min-width for classical style */
  gap: 24px; /* Increased gap for classical style */
  padding: 12px;
}
</style>