using InsuranceClaims.Helpers;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceClaims.Controllers
{
    public class SettlementController : Controller
    {
        private readonly ISettlementService _settlementService;
        private readonly IClaimService      _claimService;
        private readonly ClaimRepository    _claimRepo;

        public SettlementController(ISettlementService settlementService,
            IClaimService claimService, ClaimRepository claimRepo)
        {
            _settlementService = settlementService;
            _claimService      = claimService;
            _claimRepo         = claimRepo;
        }

        private bool IsAdmin() =>
            HttpContext.Session.IsLoggedIn() && HttpContext.Session.GetUserRole() == "Admin";

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            try
            {
                var claims = await _settlementService.GetClaimsReadyForAdminAsync();
                return View(claims);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading settlement data: {ex.Message}";
                return View(new List<InsuranceClaims.Models.Entities.Claim>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Process(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            try
            {
                if (id <= 0) { TempData["Error"] = "Invalid claim ID."; return RedirectToAction("Index"); }

                var claim = await _claimRepo.GetByIdAsync(id);
                if (claim == null) { TempData["Error"] = $"Claim #{id} not found."; return RedirectToAction("Index"); }

                if (claim.ClaimStatus != InsuranceClaims.Enums.ClaimStatus.APPROVED)
                {
                    TempData["Error"] = $"Claim #{id} must be APPROVED first.";
                    return RedirectToAction("Index");
                }

                decimal assessedAmount = claim.ClaimAmount; 
                string  assessedBy    = "Not assessed";

                if (claim.Assessments.Any())
                {
                    var latest = claim.Assessments.OrderByDescending(a => a.AssessedAt).First();
                    assessedAmount = latest.AssessedAmount;
                    assessedBy    = latest.Surveyor?.Name ?? "Surveyor";
                }

                ViewBag.ClaimId        = id;
                ViewBag.ClaimedAmount  = claim.ClaimAmount;     
                ViewBag.AssessedAmount = assessedAmount;          
                ViewBag.AssessedBy     = assessedBy;
                ViewBag.CustomerName   = claim.Customer?.Name;
                ViewBag.FraudRisk      = claim.FraudCheck?.RiskFlag.ToString() ?? "Not checked";
                ViewBag.FraudScore     = claim.FraudCheck?.FraudScore.ToString() ?? "—";

                return View(new SettlementViewModel { ClaimId = id, SettlementAmount = assessedAmount });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading claim: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(SettlementViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            try
            {
                if (!ModelState.IsValid) { ViewBag.ClaimId = model.ClaimId; return View(model); }
                var (success, message) = await _settlementService.ProcessSettlementAsync(model);
                TempData[success ? "Success" : "Error"] = message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing settlement: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int claimId)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            try
            {
                var (success, message) = await _settlementService.ApproveClaimAsync(claimId, _claimService);
                TempData[success ? "Success" : "Error"] = message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error approving claim: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int claimId, string remarks)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Home");
            try
            {
                var (success, message) = await _settlementService.RejectClaimAsync(claimId, remarks, _claimService);
                TempData[success ? "Success" : "Error"] = message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error rejecting claim: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
