import { ref, onMounted, onUnmounted } from 'vue';

/**
 * 訂單 Modal 管理的可重用組合函數
 * @param {string} modalId - Modal 的 DOM ID
 * @returns {Object} Modal 相關的響應式數據和方法
 */
export function useOrderModal(modalId = 'orderDetailModal') {
  const selectedItem = ref(null);

  /**
   * 開啟詳情 Modal
   * @param {string} orderNumber - 訂單號碼
   * @param {Array} allItems - 所有項目的陣列
   */
  const viewDetails = (orderNumber, allItems) => {
    const item = allItems.find(item => item.orderNumber === orderNumber);
    if (!item) return;
    selectedItem.value = item;
  };

  /**
   * Modal 隱藏後清除資料
   */
  const handleModalHidden = () => {
    selectedItem.value = null;
  };

  /**
   * 設置 Modal 事件監聽器
   */
  const setupModalListeners = () => {
    const modalEl = document.getElementById(modalId);
    if (modalEl) {
      modalEl.addEventListener('hidden.bs.modal', handleModalHidden);
    }
  };

  /**
   * 清除 Modal 事件監聽器
   */
  const cleanupModalListeners = () => {
    const modalEl = document.getElementById(modalId);
    if (modalEl) {
      modalEl.removeEventListener('hidden.bs.modal', handleModalHidden);
    }
  };

  onMounted(() => {
    setupModalListeners();
  });

  onUnmounted(() => {
    cleanupModalListeners();
  });

  return {
    selectedItem,
    viewDetails,
    handleModalHidden,
    setupModalListeners,
    cleanupModalListeners
  };
}