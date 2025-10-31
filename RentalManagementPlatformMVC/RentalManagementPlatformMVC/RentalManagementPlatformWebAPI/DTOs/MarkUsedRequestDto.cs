namespace RentalManagementPlatformWebAPI.DTOS
{
    public class MarkUsedRequestDto
    {
        public int CouponId { get; set; }
        public int UserId { get; set; }
        
        /// <summary>
        /// 關聯的訂單ID，用於追蹤此優惠券用在哪一筆訂單
        /// </summary>
        public int BookingId { get; set; }
    }
}