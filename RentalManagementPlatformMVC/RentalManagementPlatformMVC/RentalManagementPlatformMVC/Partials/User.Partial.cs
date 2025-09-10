using RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs;

namespace RentalManagementPlatformMVC.Models
{
	public partial class User
	{
		public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
		public ICollection<RoomList> RoomLists { get; set; } = new List<RoomList>();
    }
}