import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import {
  getPublicCoupons,
  getUserCoupons,
  redeemCoupon,
  markCouponUsed
} from '@/services/CouponService'

/** @typedef {import('@/types/coupon').Coupon} Coupon */

export const useCouponStore = defineStore('coupon', () => {
  /** -----------------------
   * 狀態 (State)
   * ----------------------- */
  const publicCoupons = ref(/** @type {Coupon[]} */ ([]))   // 所有可領取的公開優惠券
  const userCoupons = ref(/** @type {Coupon[]} */ ([]))     // 該使用者已領取的優惠券
  const loading = ref(false)
  const error = ref(null)

  /** -----------------------
   * 動作 (Actions)
   * ----------------------- */

  // 取得所有公開優惠券
  const fetchPublicCoupons = async () => {
    loading.value = true
    try {
      const data = await getPublicCoupons()
      publicCoupons.value = data
    } catch (err) {
      console.error('取得公開優惠券失敗:', err)
      error.value = err
    } finally {
      loading.value = false
    }
  }

  // 取得使用者優惠券
  const fetchUserCoupons = async (userId) => {
    loading.value = true
    try {
      const data = await getUserCoupons(userId)
      userCoupons.value = data
    } catch (err) {
      console.error('取得使用者優惠券失敗:', err)
      error.value = err
    } finally {
      loading.value = false
    }
  }

  // 領取優惠券:呼叫 API 領取後自動刷新清單
  const claimCoupon = async (userId, discountCode) => {
    try {
      const result = await redeemCoupon(userId, discountCode)
      return result
    } catch (err) {
      console.error('領取優惠券失敗:', err)
      throw err
    }
  }

  // 標記優惠券為已使用:標記後自動刷新使用者優惠券清單
  const useCoupon = async (userId, couponId) => {
    try {
      const success = await markCouponUsed({ userId, couponId })
      if (success) {
        // 更新使用者的優惠券列表
        await fetchUserCoupons(userId)
      }
      return success
    } catch (err) {
      console.error('標記優惠券為已使用失敗:', err)
      throw err
    }
  }

  /** -----------------------
   * 計算屬性 (Getters)
   * 將常用的過濾/分類邏輯放在這裡，讓元件只需讀取結果
   * ----------------------- */

  //取得「可使用」、「已過期」、「已使用」的優惠券清單
  const activeUserCoupons = computed(() =>
    userCoupons.value.filter(c => c.status === 'active')
  )

  //取得「已過期」的優惠券清單
  const expiredUserCoupons = computed(() =>
    userCoupons.value.filter(c => c.status === 'expired')
  )

  //取得「已使用」的優惠券清單
  const usedUserCoupons = computed(() =>
    userCoupons.value.filter(c => c.status === 'used')
  )

  /** -----------------------
   * 回傳對外介面
   * 透漏狀態、方法和計算屬性給元件使用
   * ----------------------- */
  return {
    // 狀態
    publicCoupons,
    userCoupons,
    loading,
    error,

    // 方法
    fetchPublicCoupons,
    fetchUserCoupons,
    claimCoupon,
    useCoupon,

    // 計算屬性
    activeUserCoupons,
    expiredUserCoupons,
    usedUserCoupons,
  }
})
