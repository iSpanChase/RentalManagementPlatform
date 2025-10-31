<template>
  <div :class="['coupon-card', statusClass]">
    <div class="coupon-left">
      <div class="coupon-icon">
        <!-- Assuming FontAwesome is available and configured -->
        <font-awesome-icon icon="house" />
        <span>優惠券</span>
      </div>
      <div class="coupon-type"></div>
    </div>
    <div class="coupon-right">
      <div class="coupon-header">
        <h3 class="coupon-name">{{ coupon.couponName }}</h3>
        <RedeemButton
          :couponId="coupon.couponId"
          :couponStatus="derivedCouponStatus"
          :disabled="!coupon.isAvailable"
          :userId="userId"
        />
      </div>
      <!-- <p v-if="coupon.claimError" class="text-danger mt-1 small">{{ coupon.claimError }}</p> -->
      <p class="coupon-perk">{{ formattedDiscount }}</p>
      <p class="coupon-min-spend">低消 ${{ coupon.lowSpend }}起</p>
      <div class="coupon-footer">
        <span class="coupon-expiry">
          <font-awesome-icon icon="clock" /> {{ formattedExpiryDate }}
        </span>
        <a href="#" class="usage-instructions" @click.prevent="showDescriptionModal = true">使用說明</a>
      </div>
    </div>
  </div>
  <!-- Coupon Description Modal -->
  <CouponDescriptionModal
    :show="showDescriptionModal"
    :description="coupon.description"
    @close="showDescriptionModal = false"
  />
</template>

<script setup>
import RedeemButton from './RedeemButton.vue'
import { computed } from 'vue'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { library } from '@fortawesome/fontawesome-svg-core'
import { faHouse, faClock } from '@fortawesome/free-solid-svg-icons'
import CouponDescriptionModal from './CouponDescriptionModal.vue' // Import the modal component
import { ref } from 'vue' // Import ref for reactive state

library.add(faHouse, faClock)

const showDescriptionModal = ref(false) // Reactive state for modal visibility

const props = defineProps({
  coupon: { type: Object, required: true },
  userId: { type: Number, required: true },
});

const statusClass = computed(() => {
  if (props.coupon.isClaiming) return 'status-claiming';
  if (props.coupon.isRedeemed) return 'status-redeemed';
  if (props.coupon.isExpired) return 'status-expired';
  if (props.coupon.isAvailable) return 'status-available';
  return '';
})

const derivedCouponStatus = computed(() => {
  if (props.coupon.isRedeemed) return 'redeemed';
  if (props.coupon.isExpired) return 'expired';
  return 'available'; // 如果沒有被領取也沒有過期，那麼它就是可用的
});

// 格式化折扣資訊
const formattedDiscount = computed(() => {
  if(props.coupon.discountMethod === 'Percentage') return `${(100 - (props.coupon.discountQuota || 0)) / 10} 折`
  if(props.coupon.discountMethod === 'Amount') return `折抵 $${props.coupon.discountQuota}`
  return '查看詳情'
})

// 格式化最低消費
const formattedMinSpend = computed(() => {
  return props.coupon.lowSpend ? `滿 ${props.coupon.lowSpend} 元可用` : '無最低消費';
});

// 格式化過期日期
const formattedExpiryDate = computed(() => {
  const date = new Date(props.coupon.endAt);
  return `有效期限至 ${date.toLocaleDateString()}`;
});
</script>

<style scoped>
.coupon-card {
  display: flex;
  border-radius: 8px;
  overflow: hidden;
  margin: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  background-color: #fff;
}

.coupon-left {
  background-color: #28a745; /* A shade of green */
  color: #fff;
  padding: 15px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  position: relative;
  width: 120px; /* Fixed width for the left section */
  clip-path: polygon(0 0, 100% 0, 100% 100%, 0% 100%); /* Basic rectangle, will add perforation */
}

/* Perforation effect for the left side */
.coupon-left::after {
  content: '';
  position: absolute;
  right: -10px; /* Adjust to control how much it overlaps */
  top: 0;
  bottom: 0;
  width: 20px; /* Width of the perforation area */
  background: radial-gradient(circle at 0 50%, transparent 8px, #28a745 8px) repeat-y;
  background-size: 100% 20px; /* Adjust 20px for the size of each 'tear' */
}

.coupon-icon {
  font-size: 3em;
  margin-bottom: 5px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.coupon-icon span {
  font-size: 0.4em;
  font-weight: bold;
}

.coupon-type {
  font-size: 0.9em;
}

.coupon-right {
  flex-grow: 1;
  padding: 15px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.coupon-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 5px;
}

.coupon-name {
  font-size: 1.1em;
  font-weight: bold;
  color: #333;
  margin: 0;
  flex-grow: 1; /* 允許標題佔據可用空間 */
  min-width: 0; /* 允許標題在空間不足時縮小 */
  overflow: hidden; /* 隱藏超出部分的文字 */
  white-space: nowrap; /* 防止文字換行 */
  text-overflow: ellipsis; /* 超出部分顯示省略號 */
}

.coupon-perk {
  font-size: 1.2em;
  font-weight: bold;
  color: #e61e4d; /* Reddish color for discount */
  margin-bottom: 5px;
}

.coupon-min-spend {
  font-size: 0.85em;
  color: #717171;
  margin-bottom: 10px;
}

.coupon-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.8em;
  color: #717171;
}

.coupon-expiry svg {
  margin-right: 5px;
}

.usage-instructions {
  color: #007bff; /* Blue link */
  text-decoration: none;
}

/* Status specific styles */
.coupon-card.status-expired {
  filter: grayscale(100%);
  opacity: 0.7;
  cursor: not-allowed;
}

.coupon-card.status-expired .usage-instructions {
  pointer-events: none;
  color: #aaa;
}

.status-redeemed .coupon-left {
  background-color: #aaa; /* Grey out for redeemed */
}

.status-redeemed .coupon-right {
  opacity: 0.7; /* Opacity for redeemed */
}

.status-expired .coupon-left {
  background-color: #aaa; /* Grey out for expired */
}

.coupon-card.status-claiming {
  filter: brightness(90%); /* 領取中時稍微變暗 */
  cursor: wait; /* 鼠標顯示等待狀態 */
}

/* Style for RedeemButton to match the image's button */
.coupon-header .redeem-button {
  border: 1px solid #28a745; /* Green border */
  color: #28a745;
  background-color: transparent;
  padding: 5px 10px;
  border-radius: 4px;
  font-size: 0.8em;
}

.coupon-header .redeem-button:disabled {
  border-color: #aaa;
  color: #aaa;
}
</style>
