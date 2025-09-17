//資料存取層,負責處理Booking的讀取

using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;

namespace RentalManagementPlatformMVC.Areas.Management.Repository
{
	public class BookingReadRepository: IBookingReadRepository
	{
		private readonly DbContext _db;//使用EF Core的DbContext來操作資料庫
		public BookingReadRepository(DbContext db) => _db = db;//建構函式注入：讓外部（例如 DI 容器）提供 DbContext 實例，支援測試與模組化。
		//而不是在new一個private readonly DbContext _db = new DbContext();(不好維護)


		// 計算該使用者的歷史訂單數量 (判斷是否為新用戶用)
		public async Task<int> CountByUserId(int userId)//非同步方式回傳(int)查詢的userId
		{
			return await _db.Set<Models.Booking>()//db.Set查找Booking資料表
			.CountAsync(b => b.GuestId == userId);//條件篩選 GuestId = userId 的筆數 (假如為0就是新用戶)
			//CountAsync => 是EF提供的非同步方式,用來計算資料筆數
		}


		//根據booking編號,要chack訂單存不存在,存在回傳訂單 或 不存在傳回null
		public async Task<Models.Booking?> GetByIdAsync(int bookingId)
		{
			return await _db.Set<Models.Booking>()//db.Set查找Booking資料表
				.FirstOrDefaultAsync(b => b.BookingId == bookingId);//找出一筆BookingId,與booking編號相同回傳值
		}

		public IQueryable<Models.Booking> Query()//提供彈性查詢,讓service加入新的條件
		{
			return _db.Set<Models.Booking>().AsQueryable();
		}
	}
}
