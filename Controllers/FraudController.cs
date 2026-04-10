using InsuranceClaims.Enums;
using InsuranceClaims.Helpers;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceClaims.Controllers
{
    public class FraudController : Controller
    {
        private readonly IFraudService   _fraudService;
        private readonly ClaimRepository _claimRepo;

        public FraudController(IFraudService fraudService, ClaimRepository claimRepo)
        {
            _fraudService = fraudService;
            _claimRepo    = claimRepo;
        }

        private bool IsOfficer() => HttpContext.Session.IsLoggedIn() && HttpContext.Session.GetUserRole() == "Officer";

        public async Task<IActionResult> Index()
        {
            if (!IsOfficer()) return RedirectToAction("AccessDenied", "Home");
            var claims = await _fraudService.GetClaimsForFraudCheckAsync();
            return View(claims);
        }

        [HttpGet]
        public async Task<IActionResult> Check(int id)
        {
            if (!IsOfficer()) return RedirectToAction("AccessDenied", "Home");
            if (id <= 0) { TempData["Error"] = "Invalid claim ID."; return RedirectToAction("Index"); }

            var claim = await _claimRepo.GetByIdAsync(id);
            if (claim == null) { TempData["Error"] = $"Claim #{id} not found."; return RedirectToAction("Index"); }


            bool allVerified = claim.Documents.Any() &&
                               claim.Documents.All(d => d.VerificationStatus == VerificationStatus.VERIFIED);
            if (!allVerified)
            {
                TempData["Error"] = $"Claim #{id}: All documents must be VERIFIED before running fraud check. Please verify docs first.";
                return RedirectToAction("Index");
            }

            ViewBag.ClaimId    = id;
            ViewBag.Claim      = claim;
            ViewBag.Assessment = claim.Assessments.FirstOrDefault();
            return View(new FraudCheckViewModel { ClaimId = id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Check(FraudCheckViewModel model)
        {
            if (!IsOfficer()) return RedirectToAction("AccessDenied", "Home");
            if (model.ClaimId <= 0) { ModelState.AddModelError("", "Invalid Claim ID."); ViewBag.ClaimId = model.ClaimId; return View(model); }
            if (!ModelState.IsValid) { ViewBag.ClaimId = model.ClaimId; return View(model); }

            var (success, message) = await _fraudService.SubmitFraudCheckAsync(model);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }
    }
}
