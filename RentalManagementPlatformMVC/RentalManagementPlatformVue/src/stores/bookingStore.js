// src/stores/bookingStore.js
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';

// ==================== API 基礎設定 ====================
const API_BASE = 'https://localhost:7230/api';

// ==================== Store ====================
export const useBookingStore = defineStore('booking', () => {
  // ==================== State ====================
  const bookingDraft = ref(null);
  const isLoading = ref(false);

  // ==================== Getters ====================
  const hasBookingDraft = computed(() => bookingDraft.value !== null);

  const nights = computed(() => {
    if (!bookingDraft.value) return 0;
    const checkIn = new Date(bookingDraft.value.checkIn);
    const checkOut = new Date(bookingDraft.value.checkOut);
    const diffTime = Math.abs(checkOut - checkIn);
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  });

  const refundableDate = computed(() => {
    if (!bookingDraft.value?.checkIn) return null;
    const date = new Date(bookingDraft.value.checkIn);
    date.setDate(date.getDate() - 7);

    if (date < new Date()) {
      return '無法退款(入住日期少於7天)';
    }

    return date.toLocaleDateString('zh-TW', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  });

  const isRefundable = computed(() => {
    if (!bookingDraft.value?.checkIn) return false;
    const checkInDate = new Date(bookingDraft.value.checkIn);
    const today = new Date();
    // 將時間部分設為 0，僅比較日期
    today.setHours(0, 0, 0, 0);
    const diffTime = checkInDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays >= 7;
  });

  const subtotal = computed(() => {
    if (!bookingDraft.value) return 0;
    return bookingDraft.value.pricePerNight * nights.value;
  });

  const serviceFee = computed(() => {
    // 假設服務費為小計的 10%
    return Math.round(subtotal.value * 0.1);
  });

  const discountAmount = computed(() => {
    return bookingDraft.value?.coupon?.discountAmount || 0;
  });

  const totalPrice = computed(() => {
    return subtotal.value + serviceFee.value - discountAmount.value;
  });

  // ==================== Actions ====================

  /**
   * 設定訂房草稿
   */
  const setBookingDraft = (data) => {
    bookingDraft.value = {
      roomId: data.roomId,
      guestId: data.guestId,
      couponId: data.couponId || null,
      checkIn: data.checkIn,
      checkOut: data.checkOut,
      guestCount: data.guestCount || 1,
      roomTitle: data.roomTitle,
      roomImage: data.roomImage,
      pricePerNight: data.pricePerNight,
      coupon: data.coupon || null,
      serviceFee: data.serviceFee || 0,
    };
  };

  /** 清除草稿 */
  const clearBookingDraft = () => {
    bookingDraft.value = null;
  };

  /**
   * 建立訂單並取得綠界表單
   */
  const createBooking = async (paymentData) => {
    if (!bookingDraft.value) throw new Error('無訂房資料');

    isLoading.value = true;
    try {
      const orderData = {
        guestId: bookingDraft.value.guestId,
        roomId: bookingDraft.value.roomId,
        couponId: bookingDraft.value.couponId,
        checkIn: bookingDraft.value.checkIn,
        checkOut: bookingDraft.value.checkOut,
        guestCount: bookingDraft.value.guestCount,
        nights: nights.value,
        pricePerNight: bookingDraft.value.pricePerNight,
        subtotal: subtotal.value,
        discountAmount: discountAmount.value,
        totalPrice: totalPrice.value,
        paymentTiming: paymentData.paymentTiming,
        billingInfo: paymentData.billingInfo,
        billingAddress: paymentData.billingAddress,
        pointsRedeemed: 0,
      };

      const response = await axios.post(`${API_BASE}/bookings/create-and-pay`, orderData, {
        headers: { 'Content-Type': 'application/json' },
      });

      return {
        bookingId: response.data.bookingId,
        orderNumber: response.data.orderNumber,
        ecpayFormHtml: response.data.ecpayFormHtml,
        paymentRequired: response.data.paymentRequired,
        paymentStatus: response.data.paymentStatus,
        paymentDeadline: response.data.paymentDeadline,
      };
    } catch (error) {
      const msg = error.response?.data?.message || error.message || '建立訂單失敗';
      throw new Error(msg);
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 取得使用者所有訂單
   */
  const fetchBookingsByUser = async (userId) => {
    if (!userId) throw new Error('未提供使用者 ID');
    isLoading.value = true;
    try {
      const { data } = await axios.get(`${API_BASE}/bookings/user/${userId}`);
      return data;
    } catch (error) {
      throw new Error('載入訂單失敗');
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 取得房東所有訂單
   */
  const fetchOrdersByHost = async (hostId) => {
    if (!hostId) throw new Error('未提供房東 ID');
    isLoading.value = true;
    try {
      const { data } = await axios.get(`${API_BASE}/bookings/host/${hostId}`);
      return data;
    } catch (error) {
      throw new Error('載入房東訂單失敗');
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 根據訂單編號取得單一訂單
   */
  const fetchBookingByOrderNumber = async (orderNumber) => {
    if (!orderNumber) throw new Error('未提供訂單編號');
    isLoading.value = true;
    try {
      const { data } = await axios.get(`${API_BASE}/bookings/ordernumber/${orderNumber}`);
      return data;
    } catch (error) {
      throw new Error('找不到該訂單');
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 取得延後付款表單
   */
  const getDeferredPaymentForm = async (orderNumber) => {
    if (!orderNumber) throw new Error('未提供訂單編號');
    isLoading.value = true;
    try {
      const { data } = await axios.get(`${API_BASE}/payments/deferred/${orderNumber}`);
      return data;
    } catch (error) {
      const msg = error.response?.data?.message || '無法取得付款表單';
      throw new Error(msg);
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 取消訂單
   */
  const cancelBooking = async (bookingId) => {
    if (!bookingId) throw new Error('未提供訂單 ID');
    isLoading.value = true;
    try {
      const { data } = await axios.put(`${API_BASE}/bookings/cancel/${bookingId}`);
      return data;
    } catch (error) {
      const msg = error.response?.data?.message || '取消訂單失敗';
      throw new Error(msg);
    } finally {
      isLoading.value = false;
    }
  };

  // ==================== Return ====================
  return {
    // State
    bookingDraft,
    isLoading,

    // Getters
    hasBookingDraft,
    nights,
    refundableDate,
    isRefundable,
    subtotal,
    serviceFee,
    discountAmount,
    totalPrice,

    // Actions
    setBookingDraft,
    clearBookingDraft,
    createBooking,
    fetchBookingsByUser,
    fetchOrdersByHost,
    fetchBookingByOrderNumber,
    getDeferredPaymentForm,
    cancelBooking,
  };
}, {
  persist: {
    paths: ['bookingDraft'],
    storage: sessionStorage,
  },
});
