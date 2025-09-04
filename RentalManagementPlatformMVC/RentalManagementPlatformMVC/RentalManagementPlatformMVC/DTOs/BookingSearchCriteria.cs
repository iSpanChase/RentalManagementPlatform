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

        // 訂單狀態
        public string Status { get; set; }

        // 訂房姓名
        public string GuestName { get; set; }

        // 房間
        public string Room { get; set; }

        // 價格區間
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // 排序
        public string SortBy { get; set; } = "CreatedAt";
        public bool IsDescending { get; set; } = true;
    }
}
