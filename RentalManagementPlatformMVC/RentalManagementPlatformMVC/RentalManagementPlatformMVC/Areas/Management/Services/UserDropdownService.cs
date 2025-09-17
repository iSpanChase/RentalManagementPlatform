//這邊是用來處理user下拉選單的服務
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Services
{
	public class UserDropdownService
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public UserDropdownService(RentalManagementPlatformSqlContext db)
		{
			_db = db;
		}
		public async Task<List<Models.User>> GetAllUsersAsync()
		{
			return await _db.Users
				.Where(u => u.Isverified)//這邊的意思是 u=>u.Isverified == true 用來判斷使用者是否有通過
				.ToListAsync();
		}
	}
}
