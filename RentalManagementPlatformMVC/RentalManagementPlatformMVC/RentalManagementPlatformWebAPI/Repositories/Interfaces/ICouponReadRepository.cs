//Repository層(介面)
//功能:定義Coupon資料存取的唯獨操作,實作會由Repository來完成
//原則:介面只規範規則,不包含怎麼做

using System.Linq;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repository.Interfaces;

public interface ICouponReadRepository//定義Coupon的Repository層ICouponReadRepository介面(interface)(讀取資料)
{
	//提供查詢Coupon資料的能力
	//IQueryable:是一種延遲加載的集合,不會立即執行查詢(會在資料庫直接過濾)
	//好處:只有在真正需要資料時才會去資料庫撈取,可以避免不必要的效能浪費
	IQueryable<Coupon> Query();//IQueryable<Coupon> 不會馬上查資料，而是交由 Service 層 去決定查詢條件
}

