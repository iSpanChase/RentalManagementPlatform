namespace RentalManagementPlatformMVC.DTOs.Bookings
{
	/// <summary>
	/// 用於封裝同行旅客的基本資訊，方便在訂單中顯示或傳遞。
	/// </summary>
	public class BookingGuestDto
	{
		public int BookingGuestId { get; set; }
		public string? GuestName { get; set; }
		public string? GuestIdNumber { get; set; }
	}
}
