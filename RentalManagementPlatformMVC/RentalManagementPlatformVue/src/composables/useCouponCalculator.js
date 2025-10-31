//coupon優惠券的邏輯處理,呼叫後端服務進行驗證
import { ref, computed, onMounted, watch } from 'vue';
import { getUserCoupons, validateCoupon } from '../services/CouponService';
import { useCouponStore } from '../stores/coupon.js';

/**
 * @description 處理優惠券相關所有商業邏輯的 Vue Composable
 * @param cartInfo - 一個包含訂單資訊的 Ref 物件
 * @returns - 所有畫面渲染所需的響應式狀態與方法
 */
export function useCouponCalculator(cartInfo) {
  // ----------------------------------------------------------------------------
  // 內部響應式狀態 (Internal Reactive State)
  // ----------------------------------------------------------------------------
  const userCoupons = ref([]); // 從後端獲取的原始優惠券資料
  const selectedCouponId = ref(null); // v-model 綁定 CouponSelector 的選擇結果

  const discountAmount = ref(0); // 後端驗證後回傳的折扣金額
  const selectedCouponDescription = ref(null); // 選定優惠券的描述
  const validationMessage = ref(''); // 驗證訊息
  const isError = ref(false); // 是否發生錯誤

  const couponStore = useCouponStore(); // 引入 coupon store

  // ----------------------------------------------------------------------------
  // 生命週期鉤子 (Lifecycle Hooks)
  // ----------------------------------------------------------------------------
  // 移除 onMounted 中的 fetchUserCoupons，改用 watch 監聽 userId

  // ----------------------------------------------------------------------------
  // 監聽器 (Watchers)
  // ----------------------------------------------------------------------------

  // 監聽 userId 變化，當 userId 存在時才獲取優惠券
  watch(() => cartInfo.value.userId, (newUserId) => {
    // 暫時使用硬編碼的 userId = 1，直到會員模組完成
    const userIdToFetch = newUserId || 1; 
    if (userIdToFetch) {
      fetchUserCoupons(userIdToFetch); 
    }
  }, { immediate: true });

  // 監聽使用者選擇的優惠券 ID，當 ID 變更時，觸發後端驗證
  watch(selectedCouponId, async (newId) => {
    // 當 newId 為 null 或 NaN 時，代表「不使用優惠券」
    if (newId === null || Number.isNaN(newId)) {
      resetCouponState();
      return;
    }

    const selected = userCoupons.value.find(c => c.couponId === newId);
    if (!selected) return;

    // 準備請求後端 API 的資料
    const request = {
      discountCode: selected.discountCode,
      totalAmount: cartInfo.value.totalAmount,
      leaseDays: cartInfo.value.leaseDays,
      userId: cartInfo.value.userId,
      cityId: cartInfo.value.cityId,
      useDate: cartInfo.value.useDate.toISOString(),
    };

    try {
      // 呼叫後端進行權威性驗證
      const response = await validateCoupon(request);

      if (response.isValid) {
        discountAmount.value = response.discountAmount || 0;
        selectedCouponDescription.value = selected.description; // 設定選定優惠券的描述
        couponStore.showToast(response.message || '優惠券已成功套用！', 'success');
      } else {
        resetCouponState();
        selectedCouponId.value = null;
        couponStore.showToast(`無法使用：${response.message}`, 'error');
      }
              } catch (error) {
                resetCouponState();
                selectedCouponId.value = null;
                couponStore.showToast(`驗證時發生錯誤：${error.response?.data?.message || error.message}`, 'error');
              }  });

  // 監聽外部傳入的 cartInfo，如果訂單金額或租期變動，就重新觸發一次驗證
  watch(
    () => [cartInfo.value.totalAmount, cartInfo.value.leaseDays],
    () => {
      // 如果當前已經選擇了一個優惠券，就觸發 watch(selectedCouponId) 的驗證
      if (selectedCouponId.value !== null) {
        const currentId = selectedCouponId.value;
        selectedCouponId.value = null; // 技巧：先設為 null 再設回原值，以強制觸發驗證
        selectedCouponId.value = currentId;
      }
    },
    { deep: true }
  );

  // ----------------------------------------------------------------------------
  // 計算屬性 (Computed Properties)
  // ----------------------------------------------------------------------------

  // 將從後端獲取的原始優惠券資料，轉換為 CouponSelector 元件所需的格式
  const couponOptions = computed(() => {
    return userCoupons.value.map(c => { 
      let disabled = false;
      let disabledMessage = '';

      if (c.lowSpend && cartInfo.value.totalAmount < c.lowSpend) {
        disabled = true;
        disabledMessage = `需滿 ${c.lowSpend} 元`;
      } else if (c.status !== '可使用') {
        disabled = true;
        disabledMessage = c.status;
      }

      return {
        couponId: c.couponId,
        couponName: c.couponName,
        description: c.discountMethod === 'Percentage' ? `${c.discountQuota}% 折扣` : `折抵 ${c.discountQuota} 元`,
        disabled,
        disabledMessage
      };
    });
  });

  // 最終應付金額
  const finalPrice = computed(() => {
    const price = cartInfo.value.totalAmount - discountAmount.value;
    return price < 0 ? 0 : price;
  });

  // ----------------------------------------------------------------------------
  // 方法 (Methods)
  // ----------------------------------------------------------------------------

  // 重設優惠券相關狀態
  function resetCouponState() {
    discountAmount.value = 0;
    selectedCouponDescription.value = null; // 清除選定優惠券的描述
  }

  // ----------------------------------------------------------------------------
  // 回傳 API (Return Public API)
  // ----------------------------------------------------------------------------
  return {
    // State
    couponOptions,
    selectedCouponId,
    discountAmount,
    finalPrice,
    selectedCouponDescription, // 匯出選定優惠券的描述

    // Methods
    resetCouponState,
    fetchUserCoupons // 匯出重新獲取使用者優惠券的方法
  };

  // 新增一個方法來重新獲取使用者優惠券
  async function fetchUserCoupons(userId) {
    if (!userId) return;
    try {
      userCoupons.value = await getUserCoupons(userId);
    } catch (error) {
      console.error("重新獲取使用者優惠券失敗：", error);
      validationMessage.value = "無法載入您的優惠券列表。";
      isError.value = true;
    }
  }
}
