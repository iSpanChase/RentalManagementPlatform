<!-- 這是測試coupon優惠券下拉式選單的購物車 -->
<template>
  <div class="checkout-page container mt-4">
    <div class="card">
      <div class="card-header">
        <h1>結帳頁面</h1>
      </div>
      <div class="card-body">
        <!-- 模擬購物車資訊 -->
        <div class="cart-info mb-4">
          <h4>購物車資訊</h4>
          <div class="row">
            <div class="col-md-6">
              <label for="cartTotalInput" class="form-label">商品總金額 (元)</label>
              <input type="number" id="cartTotalInput" v-model.number="cartInfo.totalAmount" class="form-control">
            </div>
            <div class="col-md-6">
              <label for="leaseDaysInput" class="form-label">租期天數</label>
              <input type="number" id="leaseDaysInput" v-model.number="cartInfo.leaseDays" class="form-control">
            </div>
          </div>
        </div>

        <!-- 優惠券選擇 -->
        <div class="coupon-section mb-4">
          <h4>選擇優惠券</h4>
          <CouponSelector
            v-model="selectedCouponId"
            :coupons="couponOptions"
          />
          <div v-if="selectedCouponDescription" class="mt-2 alert alert-info">
            已選優惠券：{{ selectedCouponDescription }}
          </div>
        </div>

        <!-- 結帳總結 -->
        <div class="summary">
          <h4>結帳總覽</h4>
          <ul class="list-group">
            <li class="list-group-item d-flex justify-content-between align-items-center">
              商品總金額
              <span>{{ cartInfo.totalAmount }} 元</span>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center text-danger">
              折抵金額
              <!-- <span>- {{ discountAmount }} 元</span> -->
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center fw-bold">
              最終應付金額
              <span class="fs-4 text-primary">{{ finalPrice }} 元</span>
            </li>
          </ul>
        </div>

        <!-- 操作按鈕 -->
        <div class="actions mt-4">
          <button class="btn btn-primary me-2" @click="handleCheckout" :disabled="isCheckingOut">
            <span v-if="isCheckingOut" class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            {{ isCheckingOut ? '處理中...' : '確認結帳' }}
          </button>
          <button class="btn btn-outline-secondary" @click="reset" :disabled="isCheckingOut">重設</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import CouponSelector from '../components/coupons/CouponSelector.vue';
import { useCouponCalculator } from '../composables/useCouponCalculator.js';
import type { CartInfo } from '../types/coupon';
import { markCouponUsed } from '../services/CouponService.js';
import { useAuthStore } from '@/stores/auth';

// ----------------------------------------------------------------------------
// 模擬購物車與使用者脈絡
// ----------------------------------------------------------------------------
const isCheckingOut = ref(false); // 控制結帳按鈕的狀態
const authStore = useAuthStore();

// 模擬訂單資訊，傳遞給 Composable
const cartInfo = ref<CartInfo>({
  userId: null, // 將通過 watch 動態設置
  cityId: 1, // 模擬地區 ID (1: 台北)
  useDate: new Date(), // 模擬預計使用日期
  totalAmount: 9000, // 模擬商品總金額
  leaseDays: 3, // 模擬租期
});

// 監聽 auth store 中的 profile，當它可用時更新 cartInfo 中的 userId
watch(() => authStore.state.profile, (profile) => {
  if (profile?.userId) {
    cartInfo.value.userId = profile.userId;
  }
}, { immediate: true, deep: true });


// ----------------------------------------------------------------------------
// 使用 Composable 獲取優惠券相關狀態與方法
// ----------------------------------------------------------------------------
const {
  couponOptions,
  selectedCouponId,
  discountAmount,
  finalPrice,
  resetCouponState,
  selectedCouponDescription, // 匯入選定優惠券的描述
  fetchUserCoupons, // 匯入重新獲取使用者優惠券的方法
} = useCouponCalculator(cartInfo);

            // ----------------------------------------------------------------------------
// 方法 (Methods)
// ----------------------------------------------------------------------------

// 模擬結帳
async function handleCheckout() {
  isCheckingOut.value = true;
  // 在真實應用中，這裡會呼叫後端 API 建立訂單，並傳入 selectedCouponId
  // 例如：await createOrder({ cart: ..., couponId: selectedCouponId.value });
  // 後端在建立訂單後，會將優惠券標記為已使用。

  // 模擬非同步處理
  await new Promise(resolve => setTimeout(resolve, 1000));

  // 如果有選擇優惠券，則標記為已使用
  if (selectedCouponId.value !== null && cartInfo.value.userId) {
    const markUsedRequest = {
      userId: cartInfo.value.userId,
      couponId: selectedCouponId.value,
    };
    const success = await markCouponUsed(markUsedRequest);
    if (!success) {
      console.error('Failed to mark coupon as used.');
      // 可以在這裡處理標記失敗的邏輯，例如提示用戶或回滾訂單
    }
  }

  alert(`結帳成功！\n總金額：${cartInfo.value.totalAmount} 元\n折抵：${discountAmount.value} 元\n應付：${finalPrice.value} 元`);
  isCheckingOut.value = false;

  // 結帳成功後，可以選擇重新載入優惠券列表或重設頁面
  reset();
}

// 重設整個頁面的狀態
async function reset() {
  cartInfo.value.totalAmount = 9000;
  cartInfo.value.leaseDays = 3;
  selectedCouponId.value = null; // 重設選中的優惠券 ID
  resetCouponState(); // 呼叫 Composable 提供的重設方法
  await fetchUserCoupons(); // 重新獲取使用者優惠券列表，以更新狀態
}

</script>

<style scoped>
.checkout-page {
  max-width: 800px;
}

.cart-info,
.coupon-section,
.summary,
.actions {
  padding: 1.5rem;
  border: 1px solid #dee2e6;
  border-radius: 0.375rem;
  margin-bottom: 1.5rem;
  background-color: #fff;
}

.card-header h1 {
  margin-bottom: 0;
}
</style>
