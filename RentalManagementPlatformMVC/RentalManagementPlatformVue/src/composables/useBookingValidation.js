import { ref } from 'vue';

/**
 * 預訂表單驗證工具
 */
export function useBookingValidation() {
  const errors = ref({});

  /**
   * 驗證 email 格式
   * @param {string} email
   * @returns {boolean}
   */
  const validateEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  /**
   * 驗證電話號碼
   * @param {string} phone
   * @returns {boolean}
   */
  const validatePhone = (phone) => {
    const phoneRegex = /^09\d{8}$|^0\d{1,2}-?\d{6,8}$/;
    return phoneRegex.test(phone.replace(/[-\s]/g, ''));
  };

  /**
   * 驗證必填欄位
   * @param {string} value
   * @returns {boolean}
   */
  const validateRequired = (value) => {
    return value && value.trim().length > 0;
  };

  /**
   * 驗證聯絡資訊
   * @param {Object} billingInfo
   * @returns {Object} { isValid, errors }
   */
  const validateBillingInfo = (billingInfo) => {
    const validationErrors = {};

    if (!validateRequired(billingInfo.name)) {
      validationErrors.name = '請輸入姓名';
    }

    if (!validateRequired(billingInfo.email)) {
      validationErrors.email = '請輸入 Email';
    } else if (!validateEmail(billingInfo.email)) {
      validationErrors.email = '請輸入有效的 Email 格式';
    }

    if (!validateRequired(billingInfo.phone)) {
      validationErrors.phone = '請輸入電話號碼';
    } else if (!validatePhone(billingInfo.phone)) {
      validationErrors.phone = '請輸入有效的電話號碼';
    }

    return {
      isValid: Object.keys(validationErrors).length === 0,
      errors: validationErrors
    };
  };

  /**
   * 驗證帳單地址
   * @param {Object} billingAddress
   * @returns {Object} { isValid, errors }
   */
  const validateBillingAddress = (billingAddress) => {
    const validationErrors = {};

    if (!validateRequired(billingAddress.street)) {
      validationErrors.street = '請輸入街道地址';
    }

    if (!validateRequired(billingAddress.city)) {
      validationErrors.city = '請輸入城市';
    }

    if (!validateRequired(billingAddress.zipCode)) {
      validationErrors.zipCode = '請輸入郵遞區號';
    }

    if (!validateRequired(billingAddress.country)) {
      validationErrors.country = '請選擇國家';
    }

    return {
      isValid: Object.keys(validationErrors).length === 0,
      errors: validationErrors
    };
  };

  /**
   * 驗證日期範圍
   * @param {Array} dateRange - [startDate, endDate]
   * @returns {Object} { isValid, error }
   */
  const validateDateRange = (dateRange) => {
    if (!dateRange || !dateRange[0] || !dateRange[1]) {
      return {
        isValid: false,
        error: '請選擇有效的入住和退房日期'
      };
    }

    const [checkIn, checkOut] = dateRange;
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (checkIn < today) {
      return {
        isValid: false,
        error: '入住日期不能早於今天'
      };
    }

    if (checkOut <= checkIn) {
      return {
        isValid: false,
        error: '退房日期必須晚於入住日期'
      };
    }

    return { isValid: true, error: null };
  };

  /**
   * 清除錯誤訊息
   * @param {string} field - 特定欄位，若未提供則清除所有錯誤
   */
  const clearErrors = (field = null) => {
    if (field) {
      delete errors.value[field];
    } else {
      errors.value = {};
    }
  };

  return {
    errors,
    validateEmail,
    validatePhone,
    validateRequired,
    validateBillingInfo,
    validateBillingAddress,
    validateDateRange,
    clearErrors
  };
}