using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repository.Interfaces;

namespace RentalManagementPlatformAPI.Repository
{
	public class CouponApiRepository : ICouponApiRepository
	{
		public readonly RentalManagementPlatformSqlContext _context;
		public CouponApiRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		//分配優惠券給使用者
		public async Task<bool> AssignCouponToUserAsync(string code, int userId)
		{
			try
			{
				var coupon = await _context.Coupons
					.FirstOrDefaultAsync(c => c.DiscountCode == code && !c.IsDeleted);
				if (coupon == null)
				{
					return false;
				}

				bool alreadyExists = await _context.CouponGuests
					.AnyAsync(cg => cg.CouponId == coupon.CouponId && cg.GuestId == userId);
				if (alreadyExists)
				{
					return false;
				}

				int lastId = await _context.CouponGuests
					.OrderByDescending(cg => cg.CouponGuestId)
					.Select(cg => cg.CouponGuestId)
					.FirstOrDefaultAsync();
				int newId = lastId + 1;

				var newRecord = new CouponGuest
				{
					CouponGuestId = newId,
					CouponId = coupon.CouponId,
					GuestId = userId,
					CreateAt = DateTime.Now
				};

				_context.CouponGuests.Add(newRecord);
				await _context.SaveChangesAsync();

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[AssignCouponToUserAsync]發生錯誤{ex.Message}");
				return false;
			}
		}

		//根據優惠碼查詢優惠券(還未刪除)
		public async Task<Coupon?> GetByCouponAsync(string code)
		{
			return await _context.Coupons.FirstOrDefaultAsync(c => c.DiscountCode == code && !c.IsDeleted);
		}

		//標記優惠券為已使用
		public async Task<bool> MarkUsedAsync(int couponId, int userId)
		{
			//查詢使用者與優惠券的關聯記錄
			var record = _context.CouponGuests.FirstOrDefault(c=>c.CouponId==couponId &&c.GuestId==userId);
			if(record==null)return false;//假如查無記錄,回傳false
			record.RemoveAt=DateTime.Now;//設定使用時間
			await _context.SaveChangesAsync();//儲存變更
			return true;
		}

		//取得所有優惠券列表
		public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
		{
			return await _context.Coupons.Where(c => !c.IsDeleted).ToListAsync();
		}
	}
}
