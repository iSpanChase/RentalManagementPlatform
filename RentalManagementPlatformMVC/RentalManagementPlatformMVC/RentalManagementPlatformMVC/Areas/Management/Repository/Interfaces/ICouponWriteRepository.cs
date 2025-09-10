using System.Linq;
using RentalManagementPlatformMVC.Models;
namespace RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces
{
	public interface ICouponWriteRepository //定義Coupon的Repository層ICouponWriteRepository介面(interface)(寫入資料)
	{
		Task AddAsync(Coupon entity);//非同步(Task)新增一筆Coupon資料到資料庫
		Task UpdateAsync(Coupon entity);//非同步(Task)更新一筆Coupon資料到資料庫
		Task SoftDeleteAsync(int id);//非同步(Task)軟刪除一筆Coupon資料到資料庫(不是真的刪除,而是標記該筆資料為已刪除)
		Task SaveChangesAsync();//非同步儲存變更
	}

}
