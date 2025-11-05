// src/stores/bookingStore.js
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import api from '@/services/http'; // 使用統一的API客戶端，已集成auth處理

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
   * 格式化入住/退房時間為業界標準時間
   * @param {string|Date} checkInDate - 入住日期
   * @param {string|Date} checkOutDate - 退房日期
   * @returns {Object} 格式化後的時間物件
   */
  const formatBookingDates = (checkInDate, checkOutDate) => {
    // 創建 UTC 日期並直接設定為目標時間
    // 這樣可以避免瀏覽器自動時區轉換

    // 入住日期 + 下午3點 (15:00)
    const checkInDateOnly = new Date(checkInDate).toISOString().split('T')[0];
    const checkInUTC = new Date(`${checkInDateOnly}T15:00:00.000Z`);

    // 退房日期 + 上午11點 (11:00)
    const checkOutDateOnly = new Date(checkOutDate).toISOString().split('T')[0];
    const checkOutUTC = new Date(`${checkOutDateOnly}T11:00:00.000Z`);

    return {
      checkIn: checkInUTC.toISOString(),
      checkOut: checkOutUTC.toISOString()
    };
  };

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
      // 格式化入住/退房時間為標準飯店時間
      const formattedDates = formatBookingDates(
        bookingDraft.value.checkIn,
        bookingDraft.value.checkOut
      );

      const orderData = {
        GuestId: bookingDraft.value.guestId,
        RoomId: bookingDraft.value.roomId,
        CouponId: bookingDraft.value.couponId || null, // 確保null而不是undefined
        CheckIn: formattedDates.checkIn,  // 下午3點入住
        CheckOut: formattedDates.checkOut, // 上午11點退房
        GuestCount: bookingDraft.value.guestCount,
        TotalPrice: paymentData.finalAmount, // 直接使用從前端傳入的、使用者看到的最終價格
        PaymentTiming: paymentData.paymentTiming,
        BillingInfo: {
          Name: paymentData.billingInfo.name,
          Email: paymentData.billingInfo.email,
          Phone: paymentData.billingInfo.phone,
          Notes: paymentData.billingInfo.notes || null,
        },
        BillingAddress: {
          Country: paymentData.billingAddress.country,
          Street: paymentData.billingAddress.street,
          Apartment: paymentData.billingAddress.apartment || null,
          City: paymentData.billingAddress.city,
          State: paymentData.billingAddress.state || null,
          ZipCode: paymentData.billingAddress.zipCode,
        },
        PointsRedeemed: 0,
      };

      const response = await api.post(`${API_BASE}/bookings/create-and-pay`, orderData, {
        headers: { 'Content-Type': 'application/json' },
      });

      return {
        bookingId: response.data.bookingId,
        orderNumber: response.data.orderNumber,
        paymentRequired: response.data.paymentRequired,
        paymentStatus: response.data.paymentStatus,
        paymentDeadline: response.data.paymentDeadline,
        ecpayFormHtml: response.data.ecpayFormHtml,
      };
    } catch (error) {
      console.error('Create booking error:', error);
      console.error('Error response:', error.response?.data);

      const msg = error.response?.data?.message || error.message || '建立訂單失敗';
      throw new Error(msg);
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 房客查看自己的預訂
   */
  const fetchMyBookings = async (authenticatedGuestId) => {
    isLoading.value = true;

    try {
      const response = await api.get(`${API_BASE}/bookings/my-bookings/${authenticatedGuestId}`);
      return response.data;
    } catch (error) {
      console.error('取得我的預訂失敗:', error);
      throw new Error(error.response?.data?.message || '取得預訂資料失敗');
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 房東查看自己的訂單
   */
  const fetchMyOrders = async (authenticatedHostId) => {
    isLoading.value = true;

    try {
      const response = await api.get(`${API_BASE}/bookings/my-orders/${authenticatedHostId}`);
      return response.data;
    } catch (error) {
      console.error('取得我的訂單失敗:', error);
      throw new Error(error.response?.data?.message || '取得訂單資料失敗');
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
      const { data } = await api.get(`${API_BASE}/bookings/ordernumber/${orderNumber}`);
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
      const { data } = await api.get(`${API_BASE}/payments/deferred/${orderNumber}`);
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
      const { data } = await api.put(`${API_BASE}/bookings/cancel/${bookingId}`);
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
    bookingDraft: bookingDraft,
    isLoading: isLoading,

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
    fetchMyBookings,
    fetchMyOrders,
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
