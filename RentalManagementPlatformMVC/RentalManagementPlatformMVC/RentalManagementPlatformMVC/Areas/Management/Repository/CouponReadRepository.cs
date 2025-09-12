//Repository層
//功能:只有負責資料存取,不做任何商業邏輯處理。
//原則:讓Service層去決定要怎麼用資料,Repository層只負責提供資料存取的功能。

using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Data;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Repository;

public class CouponReadRepository : ICouponReadRepository
//Coupon的Repository層,實作ICouponReadRepository介面
{
	private readonly RentalManagementPlatformSqlContext _db;//注入DbContext,連線到資料庫
	public CouponReadRepository(RentalManagementPlatformSqlContext db) => _db = db;//建構子注入DbContext,方便測試維護(這裡給值後不能修改)

	public IQueryable<Coupon> Query() => _db.Coupons.AsNoTracking();//實作Query方法,回傳IQueryable<Coupon>集合,AsNoTracking()適合「只讀查詢」的情境，不做修改實體
}

public class CouponWriteRepository : ICouponWriteRepository//Coupon的Repository層,實作ICouponWriteRepository介面
{

	private readonly RentalManagementPlatformSqlContext _db;//注入DbContext,連線到資料庫
	public CouponWriteRepository(RentalManagementPlatformSqlContext db) => _db = db;//建構子注入DbContext,方便測試維護(這裡給值後不能修改)

	public async Task AddAsync(Coupon entity)//新增一筆Coupon資料
	{
		var maxId = await _db.Coupons.MaxAsync(c => (int?)c.CouponId) ?? 0;
		entity.CouponId = maxId + 1; // 生成新的 CouponId
		await _db.Coupons.AddAsync(entity);//非同步新增一筆Coupon資料到資料庫
	}
	public async Task UpdateAsync(Coupon entity)//更新一筆Coupon資料
	{
		_db.Coupons.Update(entity);//同步標記為修改(Update本身為同步方法,不需要await)
		await Task.CompletedTask;//,但介面設計成非同步,所以用Task.CompletedTask來保持一致性
	}

	public async Task SoftDeleteAsync(int id)//軟刪除一筆Coupon資料
	{
		var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.CouponId == id);//從資料庫抓出要做刪除的優惠券,FirstOrDefaultAsync為非同步查詢,若找不到會回傳null
		if (coupon != null)//假如資料存在,不是null的狀態,就做軟刪除
		{
			coupon.IsDeleted = true;//該筆資料為已刪除(軟刪除)
			_db.Coupons.Update(coupon);//標記為更新(Update本身為同步方法,不需要await)
			
		}
	}

	public async Task SaveChangesAsync()//儲存所有的變更
	{ 
		await _db.SaveChangesAsync();//非同步儲存變更(實際寫入資料庫)
	}
}
