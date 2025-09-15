using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace RentalManagementPlatformMVC.Areas.FAQ.Controllers
{
    [Area("FAQ")]
    public class AdminController : Controller
    {
        private readonly IAntiforgery _af;
        public AdminController(IAntiforgery af) => _af = af;

        public IActionResult Index()
        {
            // 產生 Anti-forgery Token 給 AJAX 用（避免 400）
            var tokens = _af.GetAndStoreTokens(HttpContext);
            ViewData["RequestVerificationToken"] = tokens.RequestToken;
            return View();
        }
    }
}
