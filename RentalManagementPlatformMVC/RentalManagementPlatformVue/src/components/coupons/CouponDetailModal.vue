<template>
  <div v-if="visible" class="modal-backdrop" @click.self="close">
    <div class="modal">
      <!-- Add v-if check here to ensure coupon is not null -->
      <template v-if="coupon">
        <h2>{{ coupon.couponName }}</h2>
        <p>{{ coupon.description }}</p>
        <p>折扣: {{ formatDiscount(coupon) }}</p>
        <p>使用條件: {{ coupon.lowSpend ? '滿 ' + coupon.lowSpend + ' 元可用' : '無門檻' }}</p>
        <p>有效期: {{ formatDate(coupon.endAt) }}</p>
        <RedeemButton
          :couponId="coupon.couponId"
          :couponStatus="coupon.status"
          :disabled="!coupon.isAvailable"
          :userId="props.userId"
        />
      </template>
      <button class="close-btn" @click="close">關閉</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { defineProps, defineEmits } from 'vue';
import type { PropType } from 'vue';
import RedeemButton from './RedeemButton.vue';
import type { Coupon } from '@/types/coupon';

const props = defineProps({
  coupon: {
    type: Object as PropType<Coupon | null>,
    required: true,
  },
  visible: {
    type: Boolean,
    default: false,
  },
  userId: {
    type: Number,
    required: true,
  },
});

const emit = defineEmits(['close', 'redeem']);

function close() {
  emit('close');
}

function formatDate(date: string) {
  return date ? new Date(date).toLocaleDateString() : '無期限';
}

function formatDiscount(coupon: Coupon) {
  if (coupon.discountMethod === 'Percentage') return `${coupon.discountQuota}% OFF`;
  return `折抵 ${coupon.discountQuota} 元`;
}

function onRedeem(code: string) {
  emit('redeem', code);
}
</script>

<style scoped>
.modal-backdrop {
  position: fixed; top:0; left:0; width:100%; height:100%; background:rgba(0,0,0,0.4); display:flex; justify-content:center; align-items:center;
}
.modal { background:white; padding:20px; border-radius:10px; width:400px; max-width:90%; position:relative; }
.close-btn { margin-top:10px; background:#ccc; border:none; padding:6px 12px; border-radius:4px; cursor:pointer; }
</style>
