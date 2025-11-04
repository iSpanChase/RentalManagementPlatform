import { ref, computed, onMounted, watch } from 'vue';
import { getUserCoupons, validateCoupon } from '../services/CouponService';
import { useCouponStore } from '../stores/couponStore.js';
import { useBookingStore } from '../stores/bookingStore.js';
import { useToast } from 'vue-toastification';

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

  const toast = useToast(); // 引入 toast

  // ----------------------------------------------------------------------------
  // 生命週期鉤子 (Lifecycle Hooks)
  // ----------------------------------------------------------------------------
  // 移除 onMounted 中的 fetchUserCoupons，改用 watch 監聽 userId

  // ----------------------------------------------------------------------------
  // 監聽器 (Watchers)
  // ----------------------------------------------------------------------------

  // 監聽 userId 變化，當 userId 存在時才獲取優惠券
  watch(
    () => cartInfo.value.userId,
    (newUserId) => {
      if (newUserId) {
        fetchUserCoupons(newUserId);
      } else {
        userCoupons.value = [];
        selectedCouponId.value = null;
        resetCouponState();
      }
    },
    { immediate: true }
  );

  // 監聽使用者選擇的優惠券 ID，當 ID 變更時，觸發後端驗證
  watch(selectedCouponId, async (newId) => {
    // 當 newId 為 null 或 NaN 時，代表「不使用優惠券」
    if (newId === null || Number.isNaN(newId)) {
      resetCouponState();
      return;
    }

    const selected = userCoupons.value.find((c) => c.couponId === newId);
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
        toast.success(response.message || '優惠券已成功套用！');
      } else {
        resetCouponState();
        selectedCouponId.value = null;
        toast.error(`無法使用：${response.message}`);
      }
              } catch (error) {
                resetCouponState();
                selectedCouponId.value = null;
                toast.error(`驗證時發生錯誤：${error.response?.data?.message || error.message}`);
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
    console.log('Raw user coupons before filtering:', userCoupons.value);
    const calculateDiscountValue = (coupon) => {
      if (coupon.discountMethod?.toLowerCase() === 'amount') {
        return coupon.discountQuota;
      }
      if (coupon.discountMethod?.toLowerCase() === 'percentage') {
        return cartInfo.value.totalAmount * (1 - coupon.discountQuota / 100);
      }
      return 0;
    };
    return userCoupons.value
      .filter(c => c.status === '可使用') // Only show unused coupons
      .map(c => { 
        let disabled = false;
        let disabledMessage = '';

        if (c.lowSpend && cartInfo.value.totalAmount < c.lowSpend) {
          disabled = true;
          disabledMessage = `需滿 ${c.lowSpend} 元`;
        }

        return {
          couponId: c.couponId,
          couponName: c.couponName,
          description: c.discountMethod?.toLowerCase() === 'percentage' ? `${c.discountQuota / 10}折` : `現金折抵 ${c.discountQuota} 元`,
          disabled,
          disabledMessage,
          // Add original coupon data for sorting
          discountMethod: c.discountMethod,
          discountQuota: c.discountQuota,
        };
      })
      .sort((a, b) => {
        // 1. Sort by disabled status (eligible first)
        if (a.disabled !== b.disabled) {
          return a.disabled ? 1 : -1;
        }

        // 2. Sort by effective discount amount (descending)
        const discountA = calculateDiscountValue(a);
        const discountB = calculateDiscountValue(b);
        return discountB - discountA;
      });
  });

  // 最終應付金額
  const finalPrice = computed(() => {
    const bookingStore = useBookingStore();
    const price = cartInfo.value.totalAmount + bookingStore.serviceFee - discountAmount.value;
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
            fetchUserCoupons, // 匯出重新獲取使用者優惠券的方法
          };  // 新增一個方法來重新獲取使用者優惠券
  async function fetchUserCoupons(userId) {
    if (!userId) return;
    try {
      userCoupons.value = await getUserCoupons(userId);
    } catch (error) {
      console.error('重新獲取使用者優惠券失敗：', error);
      validationMessage.value = '無法載入您的優惠券列表。';
      isError.value = true;
    }
  }
}
