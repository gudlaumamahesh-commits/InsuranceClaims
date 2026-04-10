using InsuranceClaims.Helpers;
using InsuranceClaims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceClaims.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPolicyService _policyService;
        public HomeController(IPolicyService policyService) => _policyService = policyService;

        public async Task<IActionResult> Index()
        {
            var policies = await _policyService.GetAllPoliciesAsync();
            return View(policies);
        }

        public IActionResult AccessDenied()
        {
        
            if(HttpContext.Session.GetInt32(SessionKeys.UserId)==null)
            {
                return RedirectToAction("Login", "Account");
            }
			return View();
        }
    }
}
