<template>
  <div class="d-flex flex-column align-items-start flex-shrink-0"> <!-- 使用 flex-column 讓按鈕和錯誤訊息垂直排列 -->
    <button
      :disabled="isDisabled"
      class="btn btn-success"
      @click="() => mutate()"
    >
      <span v-if="isPending" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
      {{ buttonText }}
    </button>
  </div>
</template>

<script setup>
import { useMutation } from '@tanstack/vue-query'
import { computed } from 'vue'; // 引入 computed 用於計算屬性
import { useCouponStore } from '../../stores/coupon.js'; // 引入我們定義的 Pinia Store

// 定義元件接收的 props
const props = defineProps({
  couponId: { type: Number, required: true }, // 優惠券的唯一 ID，用於領取操作
  userId: { type: Number, required: true }, // 使用者 ID，用於領取操作
  couponStatus: { type: String, required: true }, // 優惠券的當前狀態
  disabled: { type: Boolean, default: false }, // 外部傳入的禁用狀態，例如父元件希望禁用按鈕
});

const couponStore = useCouponStore(); // 取得 Pinia Store 的實例，以便呼叫其 actions

// 使用 useMutation 來處理領取優惠券的異步操作
const { mutate, isPending, isSuccess, error } = useMutation({
  mutationFn: async () => {
    // mutationFn 是實際執行異步操作的函數
    // 它會呼叫 couponStore 中的 claimCoupon action
    const result = await couponStore.claimCoupon(props.couponId, props.userId);
    if (!result) {
      // 如果 Store 中的領取動作回傳 false (表示領取失敗)，
      // 這裡拋出錯誤會讓 useMutation 的 error 狀態被設定，以便在 UI 上顯示錯誤訊息
      throw new Error(couponStore.coupons.find(c => c.couponId === props.couponId)?.claimError || '領取失敗');
    }
    return result; // 領取成功則返回結果
  },
  // onSuccess 鉤子在 mutation 成功後觸發
  // 在這裡我們不需要使查詢失效，因為 Store 會直接更新優惠券的狀態，
  // 並且 CouponCard 元件會響應這些 Store 狀態的變化。
});

// 計算屬性：判斷按鈕是否應該被禁用
const isDisabled = computed(() => {
  // 如果外部傳入 disabled 為 true，或者正在處理中，或者已經成功，或者優惠券狀態不是 'available'，則禁用按鈕
  return props.disabled || isPending.value || isSuccess.value || props.couponStatus !== 'available';
});

// 計算屬性：根據當前狀態顯示按鈕上的文字
const buttonText = computed(() => {
  if (isPending.value) return '處理中...'; // 正在領取中
  if (isSuccess.value) return '已領取'; // 領取成功
  if (props.couponStatus === 'redeemed') return '已領取'; // 優惠券狀態為已領取
  if (props.couponStatus === 'expired') return '已過期'; // 優惠券狀態為已過期
  return '領取'; // 預設顯示 '領取'
});
</script>
