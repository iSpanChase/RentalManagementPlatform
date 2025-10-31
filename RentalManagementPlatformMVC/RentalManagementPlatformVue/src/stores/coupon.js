import { defineStore } from 'pinia';
import axios from 'axios';

// 定義後端 API 的基礎 URL
const API_BASE_URL = '/api';

export const useCouponStore = defineStore('coupon', {
  state: () => ({
    /** @type {import('../types/coupon').Coupon[]} */
    coupons: [],
    isLoading: false,
    error: null,
  }),

  actions: {
    async fetchCoupons(userId) {
      this.isLoading = true;
      this.error = null;

      try {
        const url = userId ? `${API_BASE_URL}/CouponApi/user/${userId}` : `${API_BASE_URL}/CouponApi/list`;
        const response = await axios.get(url);
        
        const transformedCoupons = response.data.map(rawCoupon => {
          const endAtDate = new Date(rawCoupon.endAt);
          const now = new Date();
          const isExpired = endAtDate < now;
          const isRedeemed = false; 
          const isAvailable = !isExpired && !isRedeemed;

          return {
            ...rawCoupon,
            isExpired,
            isRedeemed,
            isAvailable,
            isClaiming: false,
            claimError: undefined,
          };
        });
        this.coupons = transformedCoupons;
      } catch (err) {
        this.error = '獲取優惠券失敗：' + (err.message || '未知錯誤');
        console.error('Error fetching coupons:', err);
      } finally {
        this.isLoading = false;
      }
    },

    async refreshCoupons(userId) {
      await this.fetchCoupons(userId);
    },

    async claimCoupon(couponId, userId) {
      const coupon = this.coupons.find(c => c.couponId === couponId);

      if (!coupon || !coupon.isAvailable) {
        if (coupon) coupon.claimError = '此優惠券無法領取。';
        this.showToast('此優惠券無法領取。', 'error');
        return false;
      }

      coupon.isClaiming = true;
      coupon.claimError = undefined;

      try {
        const response = await axios.post(`${API_BASE_URL}/CouponApi/redeem`, { discountCode: coupon.discountCode, userId: userId });

        if (response.status === 200) {
          coupon.isRedeemed = true;
          coupon.isAvailable = false;
          this.showToast('優惠券領取成功！', 'success');
          return true;
        } else {
          coupon.claimError = response.data?.message || '領取失敗，請稍後再試。';
          this.showToast(coupon.claimError || '領取失敗，請稍後再試。', 'error');
          return false;
        }
      } catch (err) {
        coupon.claimError = '領取時發生錯誤：' + (err.response?.data?.message || err.message || '未知錯誤');
        this.showToast(coupon.claimError || '領取時發生未知錯誤', 'error');
        console.error('Error claiming coupon:', err);
        return false;
      } finally {
        coupon.isClaiming = false;
      }
    },

    showToast(message, type) {
      console.log(`[Toast - ${type.toUpperCase()}]: ${message}`);
      alert(`[${type.toUpperCase()}]: ${message}`);
    },
  },
});
