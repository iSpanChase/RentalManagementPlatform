using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly OrderDbContext _orderDbContext;

        public HomeController(ILogger<HomeController> logger, OrderDbContext orderDbContext)
        {
            _logger = logger;
            _orderDbContext = orderDbContext;
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
            if (_orderDbContext.Database.CanConnect())
            {
                return Content("連線成功");
            }
            else
            {
                return Content("連線失敗");
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
