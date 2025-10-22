import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export const useBookingStore = defineStore('booking', () => {
  // State - 訂房草稿
  const bookingDraft = ref(null);

  // Getters - 計算屬性
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

  // 計算小計
  const subtotal = computed(() => {
    if (!bookingDraft.value) return 0;
    return bookingDraft.value.pricePerNight * nights.value;
  });

  // 計算總價（含折扣）
  const totalPrice = computed(() => {
    if (!bookingDraft.value) return 0;
    return subtotal.value - (bookingDraft.value.discount || 0);
  });

  // Actions - 設定訂房資料
  const setBookingDraft = (data) => {
    bookingDraft.value = {
      propertyId: data.propertyId,
      propertyTitle: data.propertyTitle,
      propertyImage: data.propertyImage,
      pricePerNight: data.pricePerNight,
      checkIn: data.checkIn,
      checkOut: data.checkOut,
      guests: data.guests,
      discount: data.discount || 0,
    };
  };

  // 清除訂房資料
  const clearBookingDraft = () => {
    bookingDraft.value = null;
  };

  // 測試用：設定假資料
  const setMockData = () => {
    bookingDraft.value = {
      propertyId: '123',
      propertyTitle: '🏠Sonoya客用住房，安靜的satoyama旅館，每日可供一組客人私人租...',
      propertyImage: 'https://picsum.photos/120/90',
      pricePerNight: 4598,
      checkIn: '2025-12-31',
      checkOut: '2026-01-01',
      guests: 2,
      discount: 808,
    };
  };

  return {
    // State
    bookingDraft,

    // Getters
    hasBookingDraft,
    nights,
    subtotal,
    totalPrice,

    // Actions
    setBookingDraft,
    clearBookingDraft,
    setMockData,
  };
});
