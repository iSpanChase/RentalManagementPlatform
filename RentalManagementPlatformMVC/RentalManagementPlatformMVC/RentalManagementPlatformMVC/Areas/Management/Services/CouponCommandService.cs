//優惠券修改資料的操作(新增、更新、刪除)服務層
//DI 容器 → 建構子參數 → Service 私有欄位 → Repository 實作

using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Services
{
	public class CouponCommandService//類別宣告與注入
	{
		private readonly ICouponWriteRepository _writeRepo;

		//ICouponWriteRepository writeRepo注入至ICouponWriteRepository _writeRepo
		public CouponCommandService(ICouponWriteRepository writeRepo)
		{
			_writeRepo = writeRepo;
		}

		//Service只關心「要新增一個優惠券」,但不直接操作DbContext
		public async Task AddCouponAsync(Coupon coupon)
		{
			//分離責任 => 1.AddAsync:描述你要做甚麼  2.SaveChangesAsync: 何時真的提交由Service決定
			await _writeRepo.AddAsync(coupon);//標記為新增(暫存)
			await _writeRepo.SaveChangesAsync();//實際提交到資料庫
		}

		//Service只關心「要修改一個優惠券」,但不直接操作DbContext
		public async Task UpdateCouponAsync(Coupon coupon) 
		{
			await _writeRepo.UpdateAsync(coupon);//標記為修改(暫存)
			await _writeRepo.SaveChangesAsync();//實際提交到資料庫
		}

		//Service只關心「要(軟)刪除一個優惠券」,但不直接操作DbContext
		public async Task SoftDeleteCouponAsync(int id)
		{
			await _writeRepo.SoftDeleteAsync(id);//標記為(軟)刪除(暫存)
			await _writeRepo.SaveChangesAsync();//實際提交到資料庫
		}
	}
}
