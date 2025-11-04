// Defines the structure of a Coupon object used throughout the application.
export interface Coupon {
  couponId: number;
  couponName: string;
  description?: string;
  discountCode: string;
  discountMethod: 'Percentage' | 'FixedAmount';
  discountQuota: number;
  lowSpend?: number;
  startAt: string;
  endAt: string;
  status: string; // e.g., '可使用', '已過期', '已使用'
  isExpired: boolean;
  isRedeemed: boolean;
  isAvailable: boolean;
}

// Defines the structure of the shopping cart information required by the coupon calculator.
export interface CartInfo {
  userId: number;
  cityId: number;
  useDate: Date;
  totalAmount: number;
  leaseDays: number;
}