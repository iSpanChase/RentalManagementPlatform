namespace RentalManagementPlatformMVC.Models
{
	public class Users
	{
		public int UserId { get; set; }                // user_id
		public string Username { get; set; } = null!;  // username
		public string Email { get; set; } = null!;     // email
		public string Name { get; set; } = null!;      // name
		public bool? AutoSubscribe { get; set; }       // auto_subscribe
		public string PasswordHash { get; set; } = null!; // password_hash
		public string? Gender { get; set; }            // gender
		public DateTime? BirthDate { get; set; }       // birth_date
		public string? Phone { get; set; }             // phone
		public string? FullAddress { get; set; }       // full_address
		public int? Point { get; set; }                // point
		public bool? IsVerified { get; set; }          // isverified
		public string? ProfileImageUrl { get; set; }   // profile_imageurl (允許 NULL)
		public DateTime CreatedAt { get; set; }        // created_at
		public DateTime UpdatedAt { get; set; }        // updated_at
	}
}
