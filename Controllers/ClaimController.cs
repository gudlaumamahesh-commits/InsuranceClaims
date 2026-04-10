using InsuranceClaims.Enums;
using InsuranceClaims.Helpers;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceClaims.Controllers
{
    public class ClaimController : Controller
    {
        private readonly IClaimService      _claimService;
        private readonly IPolicyService     _policyService;
        private readonly ClaimRepository    _claimRepo;

        public ClaimController(IClaimService claimService, IPolicyService policyService,
            ClaimRepository claimRepo)
        {
            _claimService  = claimService;
            _policyService = policyService;
            _claimRepo     = claimRepo;
        }

        public async Task<IActionResult> MyClaims()
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (HttpContext.Session.GetUserRole() != "Customer") return RedirectToAction("AccessDenied", "Home");
            try
            {
                var claims = await _claimService.GetMyClaimsAsync(HttpContext.Session.GetUserId());

                
                var claimsWithRejectedDocs = new HashSet<int>();
                foreach (var c in claims)
                {
                    var full = await _claimRepo.GetByIdAsync(c.ClaimId);
                    if (full?.Documents.Any(d => d.VerificationStatus == VerificationStatus.REJECTED) == true)
                        claimsWithRejectedDocs.Add(c.ClaimId);
                }
                ViewBag.ClaimsWithRejectedDocs = claimsWithRejectedDocs;

                return View(claims);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading claims: {ex.Message}";
                return View(new List<InsuranceClaims.Models.Entities.Claim>());
            }
        }

        public async Task<IActionResult> AllClaims()
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            var role = HttpContext.Session.GetUserRole();
            if (role != "Officer" && role != "Admin") return RedirectToAction("AccessDenied", "Home");
            try
            {
                var claims = await _claimService.GetAllClaimsAsync();
                return View(claims);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading claims: {ex.Message}";
                return View(new List<InsuranceClaims.Models.Entities.Claim>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (HttpContext.Session.GetUserRole() != "Customer") return RedirectToAction("AccessDenied", "Home");
            try
            {
                var eligible = await _claimService.GetEligiblePoliciesForClaimAsync(HttpContext.Session.GetUserId());
                ViewBag.EligiblePolicies = eligible;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading policies: {ex.Message}";
                ViewBag.EligiblePolicies = new List<InsuranceClaims.Models.Entities.PolicyPurchase>();
                return View();
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClaimViewModel model)
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.EligiblePolicies = await _claimService.GetEligiblePoliciesForClaimAsync(HttpContext.Session.GetUserId());
                    return View(model);
                }
                var (success, message) = await _claimService.CreateClaimAsync(model, HttpContext.Session.GetUserId());
                TempData[success ? "Success" : "Error"] = message;
                if (!success)
                {
                    ViewBag.EligiblePolicies = await _claimService.GetEligiblePoliciesForClaimAsync(HttpContext.Session.GetUserId());
                    return View(model);
                }
                return RedirectToAction("MyClaims");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating claim: {ex.Message}";
                ViewBag.EligiblePolicies = await _claimService.GetEligiblePoliciesForClaimAsync(HttpContext.Session.GetUserId());
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            try
            {
                var claim = await _claimService.GetClaimDetailsAsync(id);
                if (claim == null) return NotFound();
                return View(claim);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading claim details: {ex.Message}";
                return RedirectToAction("MyClaims");
            }
        }
    }
}
