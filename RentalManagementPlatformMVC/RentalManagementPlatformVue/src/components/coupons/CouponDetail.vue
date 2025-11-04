<template>
  <div class="coupon-detail">
    <h2 class="text-2xl font-bold text-gray-800 mb-4">{{ coupon.couponName }}</h2>

    <div class="detail-item">
      <span class="label">折扣：</span>
      <span class="value text-indigo-600 font-semibold">
        {{ discountValue }} {{ discountUnit === '__IMAGE_COUPON__' ? '專屬優惠' : discountUnit }}
      </span>
    </div>

    <div class="detail-item">
      <span class="label">描述：</span>
      <span class="value">{{ coupon.description || '無詳細描述' }}</span>
    </div>

    <div class="detail-item">
      <span class="label">有效期限：</span>
      <span class="value">{{ formattedExpiryDate }}</span>
    </div>

    <div class="detail-item">
      <span class="label">狀態：</span>
      <span class="value">{{ coupon.status === 'unused' ? '可使用' : coupon.status === 'used' ? '已使用' : coupon.status === 'expired' ? '已過期' : '可領取' }}</span>
    </div>

    <!-- Add more details as needed, e.g., terms and conditions -->
    <div v-if="coupon.termsAndConditions" class="detail-item mt-4">
      <span class="label">使用條款：</span>
      <p class="value text-sm leading-relaxed">{{ coupon.termsAndConditions }}</p>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  coupon: { type: Object, required: true },
});

const discountValue = computed(() => {
  if (props.coupon.discountMethod === 'Percentage') return `${props.coupon.discountQuota}`;
  if (props.coupon.discountMethod === 'Amount') return `$${props.coupon.discountQuota}`;
  return '';
});

const discountUnit = computed(() => {
  if (props.coupon.discountMethod === 'Percentage') return '% OFF';
  if (props.coupon.discountMethod === 'Amount') return '折抵';
  return '__IMAGE_COUPON__'; // Placeholder for image, will display '優惠' in template
});

const formattedExpiryDate = computed(() => {
  const date = new Date(props.coupon.endAt);
  return date.toLocaleDateString();
});
</script>

<style scoped>
.coupon-detail {
  font-family: 'Inter', sans-serif;
  color: #333;
}

.detail-item {
  display: flex;
  margin-bottom: 8px;
  font-size: 0.95rem;
}

.label {
  font-weight: 600;
  color: #555;
  min-width: 80px; /* Align labels */
}

.value {
  color: #333;
}
</style>