using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using RentalManagementPlatformMVC.Areas.Auth.Repositories;
using RentalManagementPlatformMVC.Areas.Auth.Services;
using RentalManagementPlatformMVC.Areas.Auth.ViewModels;


namespace RentalManagementPlatformMVC.Areas.Auth.Controllers
{
    [Area("Auth")]
    [Route("[area]/[action]")]
    [AllowAnonymous] // 整個控制器預設可匿名
    public class AuthController : Controller
    {
        private readonly IAuthService _auth;
        private readonly IEmailSender? _email;

        public AuthController(IAuthService auth, IEmailSender? email = null)
        {
            _auth = auth;
            _email = email;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginVm { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var ok = await _auth.SignInAsync(HttpContext, vm.UsernameOrEmail, vm.Password, vm.RememberMe);
            if (!ok)
            {
                ModelState.AddModelError("", "帳號或密碼錯誤");
                return View(vm);
            }
            return !string.IsNullOrWhiteSpace(vm.ReturnUrl) ? Redirect(vm.ReturnUrl!) : Redirect("/");
        }

        [Authorize] // 只有登出需要已登入
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _auth.SignOutAsync(HttpContext);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            try
            {
				await _auth.RegisterAsync(vm);   // ← 呼叫新的多載
				return RedirectToAction(nameof(Login));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // 產生重設連結（若 email 不存在，服務端會回空字串；仍顯示已寄出，避免洩漏）
            var url = await _auth.CreatePasswordResetAsync(vm.Email);

            // 寄信（若有設定 IEmailSender）
            if (!string.IsNullOrWhiteSpace(url) && _email is not null)
                await _email.SendAsync(vm.Email, "重設密碼", $"請點擊以下連結重設密碼：{Request.Scheme}://{Request.Host}{url}");

            // 開發測試用：把連結放 TempData 方便點擊
            //if (!string.IsNullOrWhiteSpace(url))
            //    TempData["ResetLink"] = url;

            return RedirectToAction(nameof(ForgotPasswordSent));
        }

        [HttpGet]
        public IActionResult ForgotPasswordSent() => View();

        [HttpGet]
        public IActionResult ResetPassword(int uid, string token)
            => View(new ResetPasswordVm { UserId = uid, Token = token });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var ok = await _auth.ResetPasswordAsync(vm.UserId, vm.Token, vm.Password);
            if (!ok)
            {
                ModelState.AddModelError("", "連結已失效或不正確");
                return View(vm);
            }
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}