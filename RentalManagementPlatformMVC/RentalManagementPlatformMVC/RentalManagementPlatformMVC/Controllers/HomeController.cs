using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Models;
using System.Diagnostics;

namespace RentalManagementPlatformMVC.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly RentalManagementPlatformSqlContext _context;

		public HomeController(ILogger<HomeController> logger, RentalManagementPlatformSqlContext context)
		{
			_logger = logger;
			_context = context;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		public IActionResult TestDb()
		{
			if (_context.Database.CanConnect())
			{
				return Content("連線成功!");

			}
			else
			{
				return Content("連線失敗!");

			}
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
