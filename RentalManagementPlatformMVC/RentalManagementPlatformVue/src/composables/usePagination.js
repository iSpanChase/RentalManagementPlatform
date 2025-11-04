import { ref, computed } from 'vue';

/**
 * 分頁邏輯的可重用組合函數
 * @param {Array} items - 要分頁的項目陣列
 * @param {number} itemsPerPage - 每頁顯示的項目數，預設為 5
 * @returns {Object} 分頁相關的響應式數據和方法
 */
export function usePagination(items, itemsPerPage = 5) {
  const currentPage = ref(1);

  const totalPages = computed(() =>
    Math.ceil(items.value.length / itemsPerPage)
  );

  const paginatedItems = computed(() => {
    const startIndex = (currentPage.value - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    return items.value.slice(startIndex, endIndex);
  });

  const onPageChange = (page) => {
    if (page >= 1 && page <= totalPages.value) {
      currentPage.value = page;
    }
  };

  const resetToFirstPage = () => {
    currentPage.value = 1;
  };

  return {
    currentPage,
    totalPages,
    paginatedItems,
    onPageChange,
    resetToFirstPage
  };
}