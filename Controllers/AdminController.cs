using InsuranceClaims.Helpers;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceClaims.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService                _adminService;
        private readonly IPolicyService               _policyService;
        private readonly SurveyorAssignmentRepository _assignRepo;
        private readonly CustomerRepository           _customerRepo;

        public AdminController(IAdminService adminService, IPolicyService policyService,
            SurveyorAssignmentRepository assignRepo, CustomerRepository customerRepo)
        {
            _adminService  = adminService;
            _policyService = policyService;
            _assignRepo    = assignRepo;
            _customerRepo  = customerRepo;
        }

        private bool IsAdmin() => HttpContext.Session.IsLoggedIn() && HttpContext.Session.GetUserRole() == "Admin";

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            ViewBag.Officers    = await _adminService.GetOfficersAsync();
            ViewBag.Surveyors   = await _adminService.GetSurveyorsAsync();
            ViewBag.Policies    = await _policyService.GetAllPoliciesAsync();
            ViewBag.Assignments = await _assignRepo.GetAllWithDetailsAsync();
            return View();
        }

        public async Task<IActionResult> Customers()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            var customers = await _customerRepo.GetAllWithDetailsAsync();
            return View(customers);
        }

        [HttpGet] public IActionResult AddOfficer() { if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home"); return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOfficer(AddOfficerViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            if (!ModelState.IsValid) return View(model);
            var (s, m) = await _adminService.AddOfficerAsync(model);
            if (!s) { ModelState.AddModelError("", m); return View(model); }
            TempData["Success"] = m;
            return RedirectToAction("Index");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleOfficer(int officerId)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            var (s, m) = await _adminService.ToggleOfficerActiveAsync(officerId);
            TempData[s ? "Success" : "Error"] = m;
            return RedirectToAction("Index");
        }

        [HttpGet] public IActionResult AddSurveyor() { if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home"); return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSurveyor(AddSurveyorViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            if (!ModelState.IsValid) return View(model);
            var (s, m) = await _adminService.AddSurveyorAsync(model);
            if (!s) { ModelState.AddModelError("", m); return View(model); }
            TempData["Success"] = m;
            return RedirectToAction("Index");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSurveyor(int surveyorId)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            var (s, m) = await _adminService.ToggleSurveyorActiveAsync(surveyorId);
            TempData[s ? "Success" : "Error"] = m;
            return RedirectToAction("Index");
        }

        [HttpGet] public IActionResult AddPolicy() { if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home"); return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPolicy(AddPolicyViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            if (!ModelState.IsValid) return View(model);
            var (s, m) = await _policyService.AddPolicyAsync(model);
            TempData[s ? "Success" : "Error"] = m;
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePolicy(int policyId)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            var (s, m) = await _policyService.DeletePolicyAsync(policyId);
            TempData[s ? "Success" : "Error"] = m;
            return RedirectToAction("Index");
        }
    }
}
