import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';

export const useBookingStore = defineStore('booking', () => {
  // ==================== State ====================
  // 訂房草稿（前端建立訂單前的暫存資料）
  const bookingDraft = ref(null);

  // 載入狀態
  const isLoading = ref(false);

  // ==================== Getters ====================
  const hasBookingDraft = computed(() => bookingDraft.value !== null);

  // 計算住宿天數
  const nights = computed(() => {
    if (!bookingDraft.value) return 0;

    const checkIn = new Date(bookingDraft.value.checkIn);
    const checkOut = new Date(bookingDraft.value.checkOut);
    const diffTime = Math.abs(checkOut - checkIn);
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  });

  // 計算可退款日期（入住前7天）
  const refundableDate = computed(() => {
    if (!bookingDraft.value?.checkIn) return null;

    const checkInDate = new Date(bookingDraft.value.checkIn);
    checkInDate.setDate(checkInDate.getDate() - 7); // 入住前7天

    return checkInDate.toLocaleDateString('zh-TW', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  });

  // 計算小計（房價 x 天數）
  const subtotal = computed(() => {
    if (!bookingDraft.value) return 0;
    return bookingDraft.value.pricePerNight * nights.value;
  });

  // 計算折扣金額（根據 coupon）
  const discountAmount = computed(() => {
    if (!bookingDraft.value?.coupon) return 0;
    return bookingDraft.value.coupon.discountAmount || 0;
  });

  // 計算總價
  const totalPrice = computed(() => {
    if (!bookingDraft.value) return 0;
    return subtotal.value - discountAmount.value;
  });

  // ==================== Actions ====================
  // 設定訂房草稿（從房源頁面傳入）
  const setBookingDraft = (data) => {
    bookingDraft.value = {
      // 訂單基本資料
      roomId: data.roomId,
      guestId: data.guestId,
      couponId: data.couponId || null,
      checkIn: data.checkIn,
      checkOut: data.checkOut,
      guestCount: data.guestCount || 1,

      // 房源資訊
      roomTitle: data.roomTitle,
      roomImage: data.roomImage,
      pricePerNight: data.pricePerNight,

      // 優惠券資訊
      coupon: data.coupon || null, // { discountAmount: 808, ... }
    };
  };

  // 清除訂房資料
  const clearBookingDraft = () => {
    bookingDraft.value = null;
  };

  // 建立訂單並取得綠界付款表單
  const createBooking = async (paymentData) => {
    if (!bookingDraft.value) {
      throw new Error('沒有訂房資料');
    }

    isLoading.value = true;

    try {
      // 準備送到後端的完整資料
      const orderData = {
        // 訂房基本資料
        guestId: bookingDraft.value.guestId,
        roomId: bookingDraft.value.roomId,
        couponId: bookingDraft.value.couponId,
        checkIn: bookingDraft.value.checkIn,
        checkOut: bookingDraft.value.checkOut,
        guestCount: bookingDraft.value.guestCount,

        // 價格資訊
        nights: nights.value,
        pricePerNight: bookingDraft.value.pricePerNight,
        subtotal: subtotal.value,
        discountAmount: discountAmount.value,
        totalPrice: totalPrice.value,

        // 付款資訊
        paymentTiming: paymentData.paymentTiming,
        billingInfo: paymentData.billingInfo,
        billingAddress: paymentData.billingAddress,

        // 點數
        pointsRedeemed: 0,
      };

      console.log('送出到後端的資料：', orderData);

      // 呼叫後端 API
      const response = await axios.post(
        'https://localhost:7230/api/bookings/create-and-pay',
        orderData,
        {
          headers: {
            'Content-Type': 'application/json'
          },
        }
      );

      console.log('後端回應：', response.data);

      // 後端回傳的資料
      const { bookingId, orderNumber, ecpayFormHtml, paymentRequired, paymentStatus, paymentDeadline } = response.data;

      // 回傳結果
      return {
        bookingId,
        orderNumber,
        ecpayFormHtml,
        paymentRequired,
        paymentStatus,
        paymentDeadline,
      };
    } catch (error) {
      console.error('建立訂單失敗：', error);
      console.error('錯誤詳情：', error.response?.data);
      throw error;
    } finally {
      isLoading.value = false;
    }
  };

  // 獲取指定使用者的所有訂單
  const fetchUserBookings = async (userId) => {
    if (!userId) {
      throw new Error('未提供使用者 ID');
    }
    isLoading.value = true;
    try {
      const response = await axios.get(`https://localhost:7230/api/bookings/user/${userId}`);
      return response.data;
    } catch (error) {
      console.error(`獲取使用者 ${userId} 的訂單失敗：`, error);
      throw error;
    } finally {
      isLoading.value = false;
    }
  };

  // 為延後支付的訂單獲取付款表單
  const getDeferredPaymentForm = async (orderNumber) => {
    if (!orderNumber) {
      throw new Error('未提供訂單編號');
    }
    isLoading.value = true;
    try {
      const response = await axios.get(`https://localhost:7230/api/payments/deferred/${orderNumber}`);
      return response.data; // { success, orderNumber, ecpayFormHtml }
    } catch (error) {
      console.error(`為訂單 ${orderNumber} 獲取付款表單失敗：`, error);
      throw error;
    } finally {
      isLoading.value = false;
    }
  };

  return {
    // State
    bookingDraft,
    isLoading,

    // Getters
    hasBookingDraft,
    nights,
    refundableDate,
    subtotal,
    discountAmount,
    totalPrice,

    // Actions
    setBookingDraft,
    clearBookingDraft,
    createBooking,
    fetchUserBookings,
    getDeferredPaymentForm,
  };
});
