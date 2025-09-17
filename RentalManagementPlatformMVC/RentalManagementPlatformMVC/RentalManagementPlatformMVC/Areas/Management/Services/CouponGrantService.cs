using RentalManagementPlatformMVC.Areas.Management.ViewModels;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Services
{
	public class CouponGrantService
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public CouponGrantService(RentalManagementPlatformSqlContext db)
		{
			_db = db;
		}

		public async Task GrantCouponAsync(int couponId,List<int> userIds) 
		{
			foreach (var userId in userIds)
			{
				//檢查是否已經存在相同的CouponId和GuestId的記錄(避免同一個人拿取同張優惠券兩次)
				var exists =_db.CouponGuests.Any(cg => cg.CouponId == couponId && cg.GuestId == userId);
				if (!exists) 
				{
					_db.CouponGuests.Add(new CouponGuest
					{
						CouponId = couponId,
						GuestId = userId,
						CreateAt = DateTime.Now
					});
				}
				await _db.SaveChangesAsync();
			}
			//使用Select將每個userId轉換成CouponGuest物件
			//var record = userIds.Select(userId=>new CouponGuest
			//{
			//	CouponId = couponId,
			//	GuestId = userId,
			//	CreateAt = DateTime.Now
			//});
			//_db.CouponGuests.AddRange(record);
			//await _db.SaveChangesAsync();
		}
	}
}
