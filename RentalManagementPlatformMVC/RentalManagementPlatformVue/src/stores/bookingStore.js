import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';

export const useBookingStore = defineStore('booking', () => {
  // ==================== State ====================
  // 訂房草稿（前端建立訂單前的暫存資料）
  const bookingDraft = ref(null);

  /// ==================== Getters ====================
  const hasBookingDraft = computed(() => {
    return bookingDraft.value !== null;
  });

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
    if (!bookingDraft.value.checkIn) return null;

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

  // 計算折扣金額 (根據coupon)
  const discountAmount = computed(() => {
    if (!bookingDraft.value?.coupon) return 0;
    return bookingDraft.value.coupon.discount_amount || 0;
  });

  // 計算總價（含折扣）
  const totalPrice = computed(() => {
    if (!bookingDraft.value) return 0;
    return subtotal.value - discountAmount.value;
  });

  // Actions - 設定訂房資料
  const setBookingDraft = (data) => {
    bookingDraft.value = {
      // 資料庫欄位
      room_id: data.room_id,                    // 對應 room_id
      guest_id: data.guest_id || null,          // 對應 guest_id（登入用戶的ID）
      coupon_id: data.coupon_id || null,        // 對應 coupon_id
      check_in: data.check_in,                  // 對應 check_in
      check_out: data.check_out,                // 對應 check_out
      guest_count: data.guest_count || 1,       // 客人數量（這個不在資料庫，僅前端使用）

      // 房源資訊（從 ROOM 表查詢來的）
      room_title: data.room_title,              // 房源標題
      room_image: data.room_image,              // 房源圖片
      price_per_night: data.price_per_night,    // 每晚價格

      // 優惠券資訊（從 COUPON 表查詢來的，可選）
      coupon: data.coupon || null,              // { discount_amount: 808, ... }
    };
  };

  // 清除訂房資料
  const clearBookingDraft = () => {
    bookingDraft.value = null;
  };
0
  // 建立訂單 (送到後端)
  const createBooking = async () => {
    if (!bookingDraft.value) {
      throw new Error('沒有訂房資料');
    };

    try {
      // 準備送到後端的資料
      const bookingData = {
        room_id: bookingDraft.value.room_id,
        guest_id: bookingDraft.value.guest_id,
        coupon_id: bookingDraft.value.coupon_id,
        check_in: bookingDraft.value.check_in,
        check_out: bookingDraft.value.check_out,
        total_price: totalPrice.value,
        // order_number, status, created_at 等由後端自動生成
      }

      // 呼叫 API 送到後端
      const response = await axios.post('/api/bookings', bookingData);

      // 後端回傳完整的訂單資料 (包含導覽屬性)
      const booking = response.data;

      // 清除草稿
      clearBookingDraft();

      // 回傳訂單資料
      return booking;
    } catch (error) {
      console.error('建立訂單失敗:', error);
      throw error;
    };
  }

  // 測試用：設定假資料
  const setMockData = () => {
    bookingDraft.value = {
      room_id: 123,
      guest_id: 456,
      coupon_id: 789,
      check_in: '2025-12-31',
      check_out: '2026-01-01',
      guest_count: 2,

      // 房源資訊
      room_title: '🏠Sonoya客用住房，安靜的satoyama旅館，每日可供一組客人私人租...',
      room_image: 'https://picsum.photos/120/90',
      price_per_night: 4598,

      // 優惠券資訊
      coupon: {
        discount_amount: 808,
      },
    };
  };

  return {
    // State
    bookingDraft,

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
    setMockData,
  };
});
