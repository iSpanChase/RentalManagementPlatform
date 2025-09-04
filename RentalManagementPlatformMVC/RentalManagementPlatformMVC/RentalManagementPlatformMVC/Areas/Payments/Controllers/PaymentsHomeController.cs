using Microsoft.AspNetCore.Mvc;

namespace RentalManagementPlatformMVC.Areas.Payments.Controllers
{
	public class PaymentsHomeController : Controller
	{
		[Area("Payments")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
