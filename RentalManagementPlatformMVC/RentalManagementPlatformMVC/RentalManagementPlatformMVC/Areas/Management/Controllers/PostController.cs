using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.Services;
using RentalManagementPlatformMVC.Areas.Management.Services.Interfaces;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Controllers
{
	[Area("Management")]
	public class PostController:Controller
	{
		public readonly IPostQueryService _service;
		public readonly RentalManagementPlatformSqlContext _db;

		public PostController(IPostQueryService service, RentalManagementPlatformSqlContext db)
		{
			_service = service;
			_db = db;
		}
		public async Task<IActionResult> Index(int pageIndex=1,int pageSize=10,string? title=null, string? username=null, string? district=null, string? status=null, int? categoryId = null)
		{
			var (data, totalCount) = await _service.GetAllPostsVmAsync(pageIndex, pageSize, title, username, district, categoryId?.ToString());
			// 呼叫 Service 取得過濾後的資料
			//var postsVm = await _service.GetAllPostsVmAsync(title, username, district, status);

			ViewBag.TotalCount=totalCount;
			ViewBag.PageSize=pageSize;
			ViewBag.PageIndex=pageIndex;

			ViewBag.TitleFilter = title;
			ViewBag.UsernameFilter = username;
			ViewBag.DistrictFilter = district;
			ViewBag.StatusFilter = status;
			ViewBag.CategoryFilter = categoryId;

			ViewBag.Categories=await _db.Categories.Where(c=>c.IsActive==true).ToListAsync();

			// 保留篩選條件，回傳給 View 顯示
			//ViewData["TitleFilter"] = title;
			//ViewData["UsernameFilter"] = username;
			//ViewData["DistrictFilter"] = district;
			//ViewData["StatusFilter"] = status;

			return View(data);
		}
		[HttpPost]
		public async Task<IActionResult> UpdateStatus(int postId, string newStatus)
		{
			await _service.UpdateStatusAsync(postId, newStatus);
			return RedirectToAction("Index");
		}
	}
}
