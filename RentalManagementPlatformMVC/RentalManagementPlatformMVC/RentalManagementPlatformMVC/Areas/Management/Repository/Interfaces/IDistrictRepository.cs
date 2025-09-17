using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces
{
	//repository介面:定義資料存取方法
	public interface IDistrictRepository
	{
		IQueryable<District>Query();//回傳Districty資料表IQueryable(延遲查詢)
	}
}
