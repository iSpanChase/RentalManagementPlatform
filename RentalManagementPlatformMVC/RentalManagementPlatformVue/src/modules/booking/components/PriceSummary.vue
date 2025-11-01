<script setup>
import { computed } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { formatPrice } from '@/composables/useBookingFormatters';
import CouponSelector from '@/components/coupons/CouponSelector.vue';
import PriceDetailsModal from './PriceDetailsModal.vue';
import { useCouponCalculator } from '@/composables/useCouponCalculator.js';

const props = defineProps({
  showCouponSelector: {
    type: Boolean,
    default: true
  },
  showTitle: {
    type: Boolean,
    default: true
  },
  showDetailsButton: {
    type: Boolean,
    default: true
  }
});

const bookingStore = useBookingStore();

// 優惠券計算
const cartInfo = computed(() => ({
  totalAmount: bookingStore.subtotal,
  leaseDays: bookingStore.nights,
  userId: bookingStore.bookingDraft?.guestId,
  useDate: new Date(bookingStore.bookingDraft?.checkIn)
}));

const {
  couponOptions,
  selectedCouponId,
  discountAmount,
  finalPrice,
} = useCouponCalculator(cartInfo);

// 暴露給父元件使用
defineExpose({
  finalPrice,
  discountAmount,
  selectedCouponId
});
</script>

<template>
  <div class="price-summary">
    <h4 v-if="showTitle">價格詳情</h4>

    <!-- 基本價格 -->
    <div class="price-row">
      <span>{{ bookingStore.nights }} 晚 x {{ formatPrice(bookingStore.bookingDraft.pricePerNight) }}</span>
      <span>{{ formatPrice(bookingStore.subtotal) }}</span>
    </div>

    <!-- 特別優惠 -->
    <div class="price-row discount" v-if="bookingStore.discountAmount > 0">
      <span>特別優惠</span>
      <span class="green">-{{ formatPrice(bookingStore.discountAmount) }}</span>
    </div>

    <!-- 優惠券選擇器 -->
    <div v-if="showCouponSelector" class="coupon-section">
      <CouponSelector
        :coupons="couponOptions"
        v-model="selectedCouponId"
      />
    </div>

    <!-- 優惠券折扣 -->
    <div v-if="discountAmount > 0" class="price-row discount">
      <span>優惠券折扣</span>
      <span class="green">-{{ formatPrice(discountAmount) }}</span>
    </div>

    <hr v-if="discountAmount > 0 || bookingStore.discountAmount > 0">

    <!-- 最終總計 -->
    <div class="price-row total">
      <strong>總計 TWD</strong>
      <strong>{{ formatPrice(finalPrice) }}</strong>
    </div>

    <!-- 價格明細按鈕 -->
    <button
      v-if="showDetailsButton"
      class="btn-details"
      data-bs-toggle="modal"
      data-bs-target="#priceSummaryDetailsModal"
    >
      價格明細
    </button>

    <!-- 額外插槽供自定義內容 -->
    <slot name="additional-content"></slot>
  </div>

  <!-- 價格明細 Modal -->
  <PriceDetailsModal
    :discount-amount="discountAmount"
    :final-price="finalPrice"
    :modal-id="'priceSummaryDetailsModal'"
  />
</template>

<style lang="scss" scoped>
.price-summary {
  h4 {
    font-size: 18px;
    margin-bottom: 16px;
    color: #222;
    font-weight: 600;
  }
}

.price-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
  font-size: 14px;
  color: #222;

  &.discount .green {
    color: #008489;
    font-weight: 600;
  }

  &.total {
    font-size: 16px;
    padding-top: 12px;
    font-weight: 600;
  }
}

.coupon-section {
  margin: 16px 0;

  :deep(.coupon-selector) {
    label {
      display: block;
      margin-bottom: 8px;
      font-weight: 600;
      color: #222;
      font-size: 16px;
    }
  }
}

hr {
  border: none;
  border-top: 1px solid #ebebeb;
  margin: 16px 0;
}

.btn-details {
  width: 100%;
  padding: 12px;
  background: white;
  border: 1px solid #222;
  border-radius: 8px;
  cursor: pointer;
  margin-top: 16px;
  font-weight: 600;
  color: #222;

  &:hover {
    background-color: #f7f7f7;
  }
}
</style>