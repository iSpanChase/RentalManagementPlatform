using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformWebAPI.DTOs
{
	public class CreateBookingDto
	{
		[Required(ErrorMessage = "必須提供房客 ID")]
		public int GuestId { get; set; }

		[Required(ErrorMessage = "必須提供房間 ID")]
		public int RoomId { get; set; }

		public int? CouponId { get; set; }

		[Required(ErrorMessage = "必須提供入住日期")]
		[DataType(DataType.Date)]
		public DateTime CheckIn { get; set; }

		[Required(ErrorMessage = "必須提供退房日期")]
		[DataType(DataType.Date)]
		public DateTime CheckOut { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "折抵點數不能為負數")]
		public int? PointsRedeemed { get; set; }        
	}
}
