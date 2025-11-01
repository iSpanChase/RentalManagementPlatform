/**
 * 預訂相關的格式化工具
 */

/**
 * 格式化日期為「2025年12月31日」
 * @param {string|Date} dateInput - ISO 日期字串或 Date 物件
 * @returns {string}
 */
export const formatDate = (dateInput, type = 'checkIn') => {
  if (!dateInput) return '';
  const date = dateInput instanceof Date ? new Date(dateInput) : new Date(dateInput);

  // 根據 type 設定時間
  if (type === 'checkIn') {
    date.setHours(15, 0, 0, 0); // 下午3點
  } else if (type === 'checkOut') {
    date.setHours(11, 0, 0, 0); // 上午11點
  }

  // 顯示日期與時間（含上下午）
  return date.toLocaleString('zh-TW', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });
};

/**
 * 將 Date 物件轉為 'YYYY-MM-DD' 字串
 * @param {Date} date
 * @returns {string}
 */
export const toISODateString = (date) => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

/**
 * 格式化價格顯示
 * @param {number} price - 價格數字
 * @param {string} currency - 貨幣單位，預設 'TWD'
 * @returns {string}
 */
export const formatPrice = (price, currency = 'TWD') => {
  if (!price && price !== 0) return '';
  return `$${Math.round(price).toLocaleString()} ${currency}`;
};

/**
 * 格式化電話號碼
 * @param {string} phone - 電話號碼
 * @returns {string}
 */
export const formatPhone = (phone) => {
  if (!phone) return '';
  return phone.replace(/(\d{4})(\d{3})(\d{3})/, '$1-$2-$3');
};
