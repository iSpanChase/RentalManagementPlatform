/**
 * 預訂相關的格式化工具
 */

/**
 * 格式化日期為「2025年12月31日」（從 UTC 轉換為台灣時間）
 * @param {string|Date} dateInput - UTC ISO 日期字串或 Date 物件
 * @returns {string}
 */
export const formatDate = (dateInput, type = 'checkIn') => {
  if (!dateInput) return '';

  // 將 UTC 時間轉換為台灣時間顯示
  const utcDate = new Date(dateInput);

  // 顯示日期與時間（自動轉換為台灣時區）
  return utcDate.toLocaleString('zh-TW', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
    timeZone: 'Asia/Taipei'  // 明確指定台灣時區
  });
};

/**
 * 格式化日期為簡短格式「12月31日」
 * @param {string|Date} dateInput - UTC ISO 日期字串或 Date 物件
 * @returns {string}
 */
export const formatDateShort = (dateInput) => {
  if (!dateInput) return '';

  const utcDate = new Date(dateInput);

  return utcDate.toLocaleDateString('zh-TW', {
    month: 'long',
    day: 'numeric',
    timeZone: 'Asia/Taipei'
  });
};

/**
 * 格式化倒數時間
 * @param {string|Date} deadline - UTC 截止時間
 * @returns {string}
 */
export const formatCountdown = (deadline) => {
  if (!deadline) return '';

  const now = new Date();
  const end = new Date(deadline);
  const diff = end - now;

  if (diff <= 0) return '已逾期';

  const days = Math.floor(diff / (1000 * 60 * 60 * 24));
  const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

  if (days > 0) {
    return `${days}天${hours}小時`;
  } else if (hours > 0) {
    return `${hours}小時${minutes}分鐘`;
  } else {
    return `${minutes}分鐘`;
  }
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
