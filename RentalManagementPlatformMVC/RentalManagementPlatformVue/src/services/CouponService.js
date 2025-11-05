import api from '../api/axiosInstance';

// 取得所有公開的優惠券清單
export const getPublicCoupons = async () => {
try{
  const response = await api.get('/CouponApi/list');
  return response.data;
} catch (error) {
    console.error('Error fetching public coupons:', error);
    return [];
  }
};

// 取得指定使用者的優惠券清單
export const getUserCoupons = async () => {
  try{
    const response = await api.get(`/CouponApi/my-coupons`);
    return response.data;
  }catch(error){
    console.error('Error fetching public coupons:', error);
    return [];
  }
};

// 領取優惠券
export const redeemCoupon = async (userId, discountCode) => {
  try{
      const response = await api.post('/CouponApi/redeem', { userId, discountCode });
      return response.data;
  } catch (error) {
      console.error('Error fetching user coupons::', error);
      return { success: false, message: '領取優惠券時發生錯誤' };
  }
};

/**
 * 呼叫後端 API 驗證優惠券的有效性
 * @param request - 包含優惠券代碼和訂單資訊的請求物件
 * @returns - 後端回傳的驗證結果
 */
export const validateCoupon = async (request) => {
  try{
      const response = await api.post('/CouponApi/validate', request);
      return response.data;
  }catch(error){
    console.error('Error validating coupon:', error);
    return { isValid: false, message: '驗證優惠券時發生錯誤'};
  }
};

/**
 * 呼叫後端 API 標記優惠券為已使用
 * @param request - 包含使用者 ID 和優惠券 ID 的請求物件
 * @returns - 操作是否成功
 */
export const markCouponUsed = async (request) => {
  try {
    const response = await api.post('/CouponApi/mark-used', request);
    return response.status === 200;
  } catch (error) {
    console.error('Error marking coupon as used:', error);
    return false;
  }
};
