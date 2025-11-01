import { ref, onMounted, onUnmounted } from 'vue';
import { Modal } from 'bootstrap';
import { useToast } from 'vue-toastification';
import { useBookingStore } from '@/stores/bookingStore';

export function useCancelBookingModal(allBookingsRef) {
  const toast = useToast();
  const bookingStore = useBookingStore();

  const cancelModalInstance = ref(null);
  const bookingToCancel = ref(null);
  const isCancelling = ref(false); // Use a separate loading state for cancellation

  /**
   * 開啟取消確認
   */
  const openCancelConfirmModal = (booking) => {
    bookingToCancel.value = booking;
    cancelModalInstance.value?.show();
  };

  /**
   * 確認取消
   */
  const confirmCancellation = async () => {
    if (!bookingToCancel.value) return;

    const bookingId = bookingToCancel.value.bookingId;
    isCancelling.value = true;

    try {
      const response = await bookingStore.cancelBooking(bookingId);
      if (response?.success && response.booking) {
        toast.success(response.message || '訂單已成功取消');
        // Update the allBookingsRef directly
        allBookingsRef.value = allBookingsRef.value.map(b =>
          b.bookingId === response.booking.bookingId ? response.booking : b
        );
      } else {
        toast.error(response?.message || '取消失敗');
      }
    } catch (error) {
      const msg = error.response?.data?.message || error.message || '取消時發生錯誤';
      toast.error(msg);
    } finally {
      isCancelling.value = false;
      cancelModalInstance.value?.hide();
      bookingToCancel.value = null;
    }
  };

  onMounted(() => {
    const cancelEl = document.getElementById('cancelConfirmModal');
    if (cancelEl) {
      cancelModalInstance.value = new Modal(cancelEl);
    }
  });

  onUnmounted(() => {
    // Clean up modal instance if needed, though Bootstrap handles most of it
    if (cancelModalInstance.value) {
      cancelModalInstance.value.dispose();
      cancelModalInstance.value = null;
    }
  });

  return {
    bookingToCancel,
    isCancelling,
    openCancelConfirmModal,
    confirmCancellation,
  };
}
