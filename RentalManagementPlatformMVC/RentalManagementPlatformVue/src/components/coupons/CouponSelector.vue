<!-- coupon優惠券下拉選單元件 -->
<template>
  <div class="coupon-selector">
    <label for="coupon-select">選擇優惠券：</label>
    <select
      id="coupon-select"
      :value="modelValue || ''"
      @change="onSelectChange"
      class="form-select"
    >
      <option value="">不使用優惠券</option>
      <option
        v-for="coupon in coupons"
        :key="coupon.couponId"
        :value="coupon.couponId"
        :disabled="coupon.disabled"
      >
        {{ coupon.couponName }}
        <span v-if="coupon.description"> ({{ coupon.description }})</span>
        <!-- <span v-if="coupon.disabled" class="text-muted"> - {{ coupon.disabledMessage }}</span> -->
      </option>
    </select>
  </div>
</template>

<script setup lang="ts">
// ----------------------------------------------------------------------------
// 這個元件被重構為一個「笨元件」(Dumb Component)。
// 它的職責是：
// 1. 接收一個格式化好的優惠券列表 (coupons)，包含由父元件決定的 disabled 狀態。
// 2. 透過 v-model 綁定父元件傳入的 selectedCouponId。
// 3. 當使用者做出選擇時，發出 'update:modelValue' 事件，將新的 couponId 回傳給父元件。
// 它不包含任何業務邏輯，例如計算折扣或判斷優惠券是否可用。
// ----------------------------------------------------------------------------

export interface CouponOption {
  couponId: number;
  couponName: string;
  description?: string;
  disabled?: boolean;
  disabledMessage?: string; // 用於在畫面上顯示為何禁用的訊息
}
defineProps<{
  coupons: CouponOption[];
  modelValue: number | null; // 支援 v-model
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: number | null): void;
}>();

function onSelectChange(event: Event) {
  const selectedId = (event.target as HTMLSelectElement).value;
  emit('update:modelValue', selectedId && selectedId !== '' ? Number(selectedId) : null);
}
</script>

<style scoped>
.coupon-selector {
  margin-bottom: 1rem;
}

option[disabled] {
  color: #aaa;
}
</style>
