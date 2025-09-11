using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Room_List.Models
{
    public class RoomDetailsViewModel
    {
        public int RoomId { get; set; }

        [Display(Name = "標題")]
        public string? Title { get; set; }

        [Display(Name = "描述")]
        public string? Description { get; set; }

        [Display(Name = "最大入住人數")]
        public int MaxGuests { get; set; }

        [Display(Name = "每晚價格")]
        public decimal PricePerNight { get; set; }

        [Display(Name = "狀態")]
        public string? Status { get; set; }

        public AddressViewModel? Address { get; set; }

        public HostViewModel? Host { get; set; }

        [Display(Name = "照片")]
        public List<string>? PhotoUrls { get; set; }

        [Display(Name = "房東ID")]
        public int? HostId { get; set; }

        [Display(Name = "城市名稱")]
        public string? CityName { get; set; }

        [Display(Name = "區域ID")]
        public int? DistrictId { get; set; }

        [Display(Name = "區域名稱")]
        public string? DistrictName { get; set; }

        [Display(Name = "地址行")]
        public string? AddressLine { get; set; }

        public GeoLocation? Geo { get; set; }

        [Display(Name = "平均評分")]
        public double? RatingAvg { get; set; }

        [Display(Name = "評論數量")]
        public int? ReviewsCount { get; set; }

        [Display(Name = "封面儲存桶")]
        public string? CoverBucket { get; set; }

        [Display(Name = "封面物件鍵")]
        public string? CoverObjectKey { get; set; }

        [Display(Name = "封面內容類型")]
        public string? CoverContentType { get; set; }

        [Display(Name = "建立時間")]
        public DateTime? CreatedAt { get; set; }

        [Display(Name = "更新時間")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "設施")]
        public List<string> Amenities { get; set; } = new();
    }
}
