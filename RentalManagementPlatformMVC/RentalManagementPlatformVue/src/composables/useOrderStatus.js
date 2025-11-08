/**
 * 訂單狀態相關的工具函數
 */

/**
 * 取得狀態文字
 * @param {string} status - 狀態代碼
 * @returns {string} 狀態文字
 */
export const getStatusText = (status) => {
  // 轉換為小寫以便比較，避免大小寫問題
  const lowerStatus = status?.toLowerCase();

  switch (lowerStatus) {
    case 'pending':
      return '處理中';
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
    case 'failed':
      return '付款失敗';
    case 'pending_review':
      return '等待審核';
    case 'confirmed':
      return '已確認';
    case 'processing':
      return '處理中';
    case 'in_progress':
      return '進行中';
    default:
      // 如果是未知狀態，嘗試直接顯示，但提供備用文字
      return status || '未知狀態';
  }
};

/**
 * 取得狀態對應的 CSS 類別
 * @param {string} status - 狀態代碼
 * @returns {string} CSS 類別名稱
 */
export const getStatusClass = (status) => {
  if (!status) return 'status-unknown';

  const lowerStatus = status.toLowerCase();

  // 將某些狀態映射到相同的樣式類別
  const statusMapping = {
    'pending': 'pending',
    'deferred': 'deferred',
    'unpaid': 'deferred', // 使用deferred的樣式
    'completed': 'completed',
    'paid': 'completed', // 使用completed的樣式
    'cancelled': 'cancelled',
    'refunded': 'refunded',
    'failed': 'failed',
    'pending_review': 'pending', // 使用pending的樣式
    'confirmed': 'completed', // 使用completed的樣式
    'processing': 'pending', // 使用pending的樣式
    'in_progress': 'pending' // 使用pending的樣式
  };

  const mappedStatus = statusMapping[lowerStatus] || 'unknown';
  return `status-${mappedStatus}`;
};

/**
 * 檢查是否可以取消
 * @param {string} status - 狀態代碼
 * @returns {boolean}
 */
export const canCancel = (status) => {
  if (!status) return false;
  const lowerStatus = status.toLowerCase();
  return !['cancelled', 'refunded', 'completed', 'failed'].includes(lowerStatus);
};

/**
 * 檢查是否可以聯繫
 * @param {string} status - 狀態代碼
 * @returns {boolean}
 */
export const canContact = (status) => {
  if (!status) return false;
  const lowerStatus = status.toLowerCase();
  return !['cancelled', 'refunded'].includes(lowerStatus);
};

/**
 * 檢查是否可以重新預訂
 * @param {string} status - 狀態代碼
 * @returns {boolean}
 */
export const canRebook = (status) => {
  if (!status) return false;
  const lowerStatus = status.toLowerCase();
  return ['cancelled', 'completed', 'refunded', 'failed'].includes(lowerStatus);
};

/**
 * 檢查是否需要付款
 * @param {string} status - 狀態代碼
 * @returns {boolean}
 */
export const needsPayment = (status) => {
  if (!status) return false;
  const lowerStatus = status.toLowerCase();
  return ['deferred', 'unpaid'].includes(lowerStatus);
};

/**
 * 調試功能：記錄未知狀態，幫助開發者識別需要添加的新狀態
 * @param {string} status - 狀態代碼
 */
export const logUnknownStatus = (status) => {
  if (!status) return;

  const lowerStatus = status.toLowerCase();
  const knownStatuses = [
    'pending', 'deferred', 'unpaid', 'completed', 'paid',
    'cancelled', 'refunded', 'failed', 'pending_review',
    'confirmed', 'processing', 'in_progress'
  ];

  if (!knownStatuses.includes(lowerStatus)) {
    console.warn(`⚠️ 發現未知的訂單狀態: "${status}" (原始值: "${status}")，請檢查是否需要在 useOrderStatus.js 中添加支援`);
  }
};
