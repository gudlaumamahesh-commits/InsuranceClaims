using InsuranceClaims.Helpers;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceClaims.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService) => _accountService = accountService;

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.IsLoggedIn()) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var (success, message) = await _accountService.RegisterAsync(model);
                if (!success) { ModelState.AddModelError("", message); return View(model); }
                TempData["Success"] = message;
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Registration failed: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.IsLoggedIn()) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var user = await _accountService.LoginAsync(model);
                if (user == null) { ModelState.AddModelError("", "Invalid email or password."); return View(model); }
                HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
                HttpContext.Session.SetString(SessionKeys.UserRole, user.Role.ToString());
                HttpContext.Session.SetString(SessionKeys.UserEmail, user.Email);
                TempData["Success"] = $"Welcome! Logged in as {user.Role}.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Login failed: {ex.Message}");
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
