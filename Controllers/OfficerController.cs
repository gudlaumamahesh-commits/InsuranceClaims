using InsuranceClaims.Helpers;
using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceClaims.Controllers
{
    public class OfficerController : Controller
    {
        private readonly ClaimRepository              _claimRepo;
        private readonly SurveyorRepository           _surveyorRepo;
        private readonly OfficerRepository            _officerRepo;
        private readonly SurveyorAssignmentRepository _assignRepo;

        public OfficerController(ClaimRepository claimRepo, SurveyorRepository surveyorRepo,
            OfficerRepository officerRepo, SurveyorAssignmentRepository assignRepo)
        {
            _claimRepo    = claimRepo;
            _surveyorRepo = surveyorRepo;
            _officerRepo  = officerRepo;
            _assignRepo   = assignRepo;
        }

        private bool IsOfficer() =>
            HttpContext.Session.IsLoggedIn() && HttpContext.Session.GetUserRole() == "Officer";

        public async Task<IActionResult> Dashboard()
        {
            if (!IsOfficer()) return RedirectToAction("AccessDenied", "Home");
            try
            {
                var claims    = await _claimRepo.GetAllAsync();
                var allAssign = await _assignRepo.GetAllWithDetailsAsync();
                ViewBag.Assignments = allAssign;
                return View(claims);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading dashboard: {ex.Message}";
                return View(new List<Claim>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> AssignSurveyor(int id)
        {
            if (!IsOfficer()) return RedirectToAction("AccessDenied", "Home");
            try
            {
                if (id <= 0) { TempData["Error"] = "Invalid claim ID."; return RedirectToAction("Dashboard"); }

                var claim = await _claimRepo.GetByIdAsync(id);
                if (claim == null) { TempData["Error"] = $"Claim #{id} not found."; return RedirectToAction("Dashboard"); }

                var existing = await _assignRepo.GetByClaimIdAsync(id);
                if (existing != null)
                {
                    TempData["Error"] = $"Claim #{id} is already assigned to Surveyor '{existing.Surveyor?.Name}'.";
                    return RedirectToAction("Dashboard");
                }

                ViewBag.Claim     = claim;
                ViewBag.Surveyors = await _surveyorRepo.GetActiveAsync(); 
                return View(new AssignSurveyorViewModel { ClaimId = id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading assign form: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSurveyor(AssignSurveyorViewModel model)
        {
            if (!IsOfficer()) return RedirectToAction("AccessDenied", "Home");
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Claim     = await _claimRepo.GetByIdAsync(model.ClaimId);
                    ViewBag.Surveyors = await _surveyorRepo.GetActiveAsync(); 
                    return View(model);
                }

                var officer = await _officerRepo.GetByUserIdAsync(HttpContext.Session.GetUserId());
                if (officer == null) { TempData["Error"] = "Officer profile not found."; return RedirectToAction("Dashboard"); }

                var assignment = new SurveyorAssignment
                {
                    ClaimId    = model.ClaimId,
                    SurveyorId = model.SurveyorId,
                    OfficerId  = officer.OfficerId,
                    Notes      = model.Notes,
                    AssignedAt = DateTime.Now
                };
                await _assignRepo.AddAsync(assignment);

                await _claimRepo.AddTrackingAsync(new ClaimTracking
                {
                    ClaimId   = model.ClaimId,
                    Status    = "SURVEYOR_ASSIGNED",
                    Remarks   = $"Surveyor assigned by officer.{(string.IsNullOrWhiteSpace(model.Notes) ? "" : " Notes: " + model.Notes)}",
                    UpdatedAt = DateTime.Now
                });

                TempData["Success"] = "Surveyor assigned. They will now see this claim in their dashboard.";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error assigning surveyor: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }
    }
}
