using Microsoft.AspNetCore.Mvc;

namespace RentalManagementPlatformMVC.Areas.UserManagement.Controllers
{
	public class UserManagementController : Controller
	{
		[Area("UserManagement")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
