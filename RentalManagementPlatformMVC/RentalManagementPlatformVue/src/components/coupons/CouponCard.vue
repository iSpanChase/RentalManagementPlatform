<template>
  <div class="coupon-card">
    <!-- Expiring Soon Banner -->
    <div v-if="coupon.isExpiringSoon" class="expiring-soon-banner">
      <span>即將到期</span>
    </div>

    <!-- Top Section -->
    <div class="top-section">
      <div class="discount-value">{{ discountValue }}</div>
      <div class="discount-unit">
      <img v-if="discountUnit === '__IMAGE_COUPON__'" :src="couponImage" alt="優惠" class="w-1 h-1" />
      <span v-else>{{ discountUnit }}</span>
    </div>
    </div>

    <!-- Middle Section -->
    <div class="middle-section" @click="$emit('showDetails', coupon)">
      <div class="coupon-name">
        <font-awesome-icon :icon="faTicketAlt" class="icon" />
        <span>{{ coupon.couponName }}</span>
      </div>
      <p class="description">{{ coupon.description || '暫無詳細說明' }}</p>
      <p class="expiry-date">有效期限至 {{ formattedExpiryDate }}</p>
    </div>

    <!-- Bottom Section -->
    <div class="bottom-section">
      <RedeemButton 
        :text="buttonState.text"
        :disabled="buttonState.disabled"
        :loading="isPending"
        :customClass="'redeem-style'"
        @click="handleAction"
      />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { faTicketAlt } from '@fortawesome/free-solid-svg-icons';
import RedeemButton from './RedeemButton.vue';
import couponImage from '@/image/coupon.png';
import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useCouponStore } from '../../stores/couponStore.js';
import { useToast } from 'vue-toastification';

const props = defineProps({
  coupon: { type: Object, required: true },
  userId: { type: Number, required: true },
});

const emit = defineEmits(['gotoUse', 'showDetails']);

// --- Date & Status Logic ---
const isClaimed = computed(() => props.coupon.status !== undefined);
const isUsed = computed(() => props.coupon.status === 'used');
const isExpired = computed(() => new Date(props.coupon.endAt) <= new Date() || props.coupon.status === 'expired');

// --- API & State Logic ---
const couponStore = useCouponStore();
const queryClient = useQueryClient();
const toast = useToast();

const { mutate, isPending } = useMutation({
  mutationFn: () => couponStore.claimCoupon(props.userId, props.coupon.discountCode),
  onSuccess: (data) => { // data will be { message: "領取成功" }
    toast.success(data.message);
    queryClient.invalidateQueries({ queryKey: ['userCoupons', props.userId] });
    queryClient.invalidateQueries({ queryKey: ['publicCoupons'] });
  },
  onError: (error) => {
    const errorMessage = error.response?.data?.message || '領取時發生未知錯誤';
    toast.error(errorMessage);
  },
});

const handleAction = () => {
  if (props.coupon.status === 'unused') {
    emit('gotoUse', props.coupon.couponId);
  } else if (!props.coupon.isRedeemed && !isClaimed.value) { // Only mutate if not already redeemed and not claimed
    mutate();
  }
};

// --- Computed Properties for Display ---
const buttonState = computed(() => {
  if (isPending.value) return { text: '處理中', disabled: true };
  if (isUsed.value) return { text: '已使用', disabled: true };
  if (isExpired.value) return { text: '已過期', disabled: true };
  if (props.coupon.isRedeemed) return { text: '已領取', disabled: true }; // New state
  if (props.coupon.status === 'unused') return { text: '使用', disabled: false };
  return { text: '領取', disabled: false };
});

const discountValue = computed(() => {
  if (props.coupon.discountMethod === 'Percentage') return `${props.coupon.discountQuota}`;
  if (props.coupon.discountMethod === 'Amount') return `$${props.coupon.discountQuota}`;
  return '';
});

const discountUnit = computed(() => {
  if (props.coupon.discountMethod === 'Percentage') return '% OFF';
  if (props.coupon.discountMethod === 'Amount') return '折抵';
  return '__IMAGE_COUPON__'; // Special string to indicate image display
});

const formattedExpiryDate = computed(() => {
  const date = new Date(props.coupon.endAt);
  return date.toLocaleDateString();
});
</script>

<style scoped>
.coupon-card {
  background: #2d3748; /* Dark Slate Gray */
  border-radius: 12px;
  box-shadow: 0 0 0 1px #D4AF37, 0 4px 12px rgba(0, 0, 0, 0.4); /* Gold border + heavier shadow */
  overflow: hidden;
  display: flex;
  flex-direction: column;
  font-family: 'Inter', sans-serif;
  width: 230px;
  position: relative; /* Needed for pseudo-elements */
  border: 2px solid transparent; /* For inner border effect */
}

.coupon-card::before,
.coupon-card::after {
  content: '';
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 20px; /* Size of the notch */
  height: 20px; /* Size of the notch */
  background: #1a202c; /* Match the bottom section background */
  border: 2px solid #D4AF37; /* Gold border */
  border-radius: 50%;
  z-index: 1; /* Ensure it's above the card content */
}

.coupon-card::before {
  left: -10px; /* Half of the width to make it centered on the edge */
}

.coupon-card::after {
  right: -10px; /* Half of the width to make it centered on the edge */
}

.top-section {
  background-color: transparent;
  color: #D4AF37; /* Gold */
  padding: 20px 16px 16px;
  text-align: center;
  display: flex;
  justify-content: center;
  align-items: baseline;
  gap: 8px;
  border-bottom: 1px dashed #D4AF37; /* Gold dashed line */
}

.discount-value {
  font-family: 'serif'; /* More elegant font */
  font-size: 3rem;
  font-weight: bold;
}

.discount-unit {
  font-size: 1.25rem;
  font-weight: 600;
}

.middle-section {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  border-bottom: 1px dashed #D4AF37;
}

.coupon-name {
  font-size: 1.1rem;
  font-weight: 600;
  color: #f7fafc; /* Off-white */
  display: flex;
  align-items: center;
  gap: 8px;
}

.icon {
  color: #D4AF37; /* Gold */
}

.description, .expiry-date {
  font-size: 0.875rem;
  color: #a0aec0; /* Lighter gray */
}

.bottom-section {
  padding: 16px;
  background-color: #1a202c; /* Even darker slate */
  margin-top: auto; /* Push to bottom */
}

/* Custom style for the redeem button */
:deep(.redeem-style) {
  width: 100%;
  background: linear-gradient(145deg, #e7c55c, #D4AF37); /* Gold gradient */
  color: #2d3748; /* Dark text */
  font-weight: bold;
  padding: 12px;
  border-radius: 8px;
  border: none;
  transition: transform 0.2s, box-shadow 0.2s;
  box-shadow: 0 2px 4px rgba(0,0,0,0.2);
}

:deep(.redeem-style:hover) {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(212, 175, 55, 0.3);
}

.expiring-soon-banner {
  position: absolute;
  top: -1px;
  right: -1px;
  background-color: #e53e3e; /* Red-600 */
  color: white;
  padding: 4px 8px;
  font-size: 0.75rem; /* 12px */
  font-weight: bold;
  border-top-right-radius: 12px;
  border-bottom-left-radius: 8px;
  z-index: 3;
  box-shadow: 0 2px 4px rgba(0,0,0,0.2);
}
</style>