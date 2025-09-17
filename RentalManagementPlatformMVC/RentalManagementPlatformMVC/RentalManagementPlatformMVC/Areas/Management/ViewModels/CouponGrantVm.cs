//用於發放優惠券的資料的暫存
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RentalManagementPlatformMVC.Areas.Management.ViewModels
{
	public class CouponGrantVm
	{
		//管理員選擇優惠券id
		public int SelectedCouponId { get; set; }

		//管理員選擇的使用者id (多選)
		//new List<int>是防禦性設計,避免有null
		public int SelectedUserIds { get; set; }
		
		//優惠券下拉選單資料(SelectListItem是用來綁定下拉是選單)
		public List<SelectListItem>Coupons { get; set; } = new List<SelectListItem>();
		
		//使用者下拉選單資料
		public List<SelectListItem>Users { get; set; } = new List<SelectListItem>();
	}
}
