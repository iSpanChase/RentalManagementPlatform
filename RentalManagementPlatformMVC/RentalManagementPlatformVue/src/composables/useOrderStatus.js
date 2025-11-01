/**
 * 訂單狀態相關的工具函數
 */

/**
 * 取得狀態文字
 * @param {string} status - 狀態代碼
 * @returns {string} 狀態文字
 */
export const getStatusText = (status) => {
  switch (status) {
    case 'deferred':
    case 'unpaid':
      return '待付款';
    case 'completed':
    case 'paid':
      return '已完成';
    case 'cancelled':
      return '已取消';
    case 'refunded':
      return '已退款';
    default:
      return status || '未知狀態';
  }
};

/**
 * 取得狀態對應的 CSS 類別
 * @param {string} status - 狀態代碼
 * @returns {string} CSS 類別名稱
 */
export const getStatusClass = (status) => {
  return `status-${status}`;
};

/**
 * 檢查是否可以取消
 * @param {string} status - 狀態代碼
 * @returns {boolean}
 */
export const canCancel = (status) => {
  return !['cancelled', 'refunded'].includes(status);
};

/**
 * 檢查是否可以聯繫
 * @param {string} status - 狀態代碼
 * @returns {boolean}
 */
export const canContact = (status) => {
  return !['cancelled', 'refunded'].includes(status);
};

/**
 * 檢查是否可以重新預訂
 * @param {string} status - 狀態代碼
 * @returns {boolean}
 */
export const canRebook = (status) => {
  return ['cancelled', 'completed', 'refunded'].includes(status);
};

/**
 * 檢查是否需要付款
 * @param {string} status - 狀態代碼
 * @returns {boolean}
 */
export const needsPayment = (status) => {
  return status === 'deferred';
};