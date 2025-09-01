namespace RentalManagementPlatformMVC.DTOs
{
    public class BookingSearchCriteria
    {
        // 日期區間 (入住日期)
        public DateTime? CheckInStartDate { get; set; }
        public DateTime? CheckInEndDate { get; set; }

        // 日期區間 (退房日期)  
        public DateTime? CheckOutStartDate { get; set; }
        public DateTime? CheckOutEndDate { get; set; }

        // 訂單編號
        public string OrderNumber { get; set; }

        // 訂單狀態 (因為你的資料庫是字串型別)
        public string Status { get; set; }

        // 旅客姓名 (從BOOKING_GUEST查詢)
        public string GuestName { get; set; }

        // 房間ID
        public int? RoomId { get; set; }

        // 價格區間
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // 分頁參數
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // 排序
        public string SortBy { get; set; } = "CreatedAt";
        public bool IsDescending { get; set; } = true;
    }
}
