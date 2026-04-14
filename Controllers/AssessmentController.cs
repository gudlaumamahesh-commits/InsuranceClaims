using InsuranceClaims.Helpers;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceClaims.Controllers
{
    public class AssessmentController : Controller
    {
        private readonly IAssessmentService _assessmentService;
        private readonly SurveyorRepository _surveyorRepo;
        private readonly ClaimRepository    _claimRepo;

        public AssessmentController(IAssessmentService assessmentService,
            SurveyorRepository surveyorRepo, ClaimRepository claimRepo)
        {
            _assessmentService = assessmentService;
            _surveyorRepo      = surveyorRepo;
            _claimRepo         = claimRepo;
        }

        public async Task<IActionResult> Index()
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (HttpContext.Session.GetUserRole() != "Surveyor") return RedirectToAction("AccessDenied", "Home");

            var surveyor = await _surveyorRepo.GetByUserIdAsync(HttpContext.Session.GetUserId());
            if (surveyor == null)
            {
                TempData["Error"] = "Surveyor profile not found.";
                return RedirectToAction("AccessDenied", "Home");
            }

            var assignments = await _assessmentService.GetAssignedClaimsForSurveyorAsync(surveyor.SurveyorId);
            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> Submit(int id)
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (HttpContext.Session.GetUserRole() != "Surveyor") return RedirectToAction("AccessDenied", "Home");

            if (id <= 0) { TempData["Error"] = "Invalid claim ID."; return RedirectToAction("Index"); }

            var claim = await _claimRepo.GetByIdAsync(id);
            if (claim == null) { TempData["Error"] = $"Claim #{id} not found."; return RedirectToAction("Index"); }

            ViewBag.ClaimId = id;
            ViewBag.Claim   = claim;
            return View(new AssessmentViewModel { ClaimId = id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(AssessmentViewModel model)
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (HttpContext.Session.GetUserRole() != "Surveyor") return RedirectToAction("AccessDenied", "Home");

            if (!ModelState.IsValid)
            {
                ViewBag.ClaimId = model.ClaimId;
                ViewBag.Claim   = await _claimRepo.GetByIdAsync(model.ClaimId);
                return View(model);
            }

            var (success, message) = await _assessmentService.SubmitAssessmentAsync(model, HttpContext.Session.GetUserId());
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }
    }
}
