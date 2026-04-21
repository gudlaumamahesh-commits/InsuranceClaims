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
        private readonly PolicyRepository   _policyRepo;
        private readonly CustomerRepository _customerRepo;

        public ClaimController(IClaimService claimService, IPolicyService policyService,
            ClaimRepository claimRepo, PolicyRepository policyRepo, CustomerRepository customerRepo)
        {
            _claimService  = claimService;
            _policyService = policyService;
            _claimRepo     = claimRepo;
            _policyRepo    = policyRepo;
            _customerRepo  = customerRepo;
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

                // Calculate remaining amounts for each policy
                var customer = await _customerRepo.GetByUserIdAsync(HttpContext.Session.GetUserId());
                var remainingAmounts = new Dictionary<int, decimal>();

                if (customer != null)
                {
                    foreach (var purchase in eligible)
                    {
                        var claims = await _policyRepo.GetClaimsByCustomerAndPolicyAsync(
                            customer.CustomerId, purchase.PolicyId);
                        
                        // Get the last renewal date
                        var lastRenewalDate = purchase.Renewals?.OrderByDescending(r => r.RenewalDate).FirstOrDefault()?.RenewalDate;
                        
                        // Only count settled claims after the last renewal
                        decimal totalSettled = claims
                            .Where(c => c.SettlementLog != null && 
                                        c.SettlementLog.SettlementStatus == Enums.SettlementStatus.PROCESSED &&
                                        (!lastRenewalDate.HasValue || c.CreatedAt >= lastRenewalDate.Value))
                            .Sum(c => c.SettlementLog!.SettlementAmount);

                        decimal remaining = (purchase.Policy?.CoverageAmount ?? 0) - totalSettled;
                        remainingAmounts[purchase.PolicyId] = remaining;
                    }
                }

                ViewBag.RemainingAmounts = remainingAmounts;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading policies: {ex.Message}";
                ViewBag.EligiblePolicies = new List<InsuranceClaims.Models.Entities.PolicyPurchase>();
                ViewBag.RemainingAmounts = new Dictionary<int, decimal>();
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
                    var customer = await _customerRepo.GetByUserIdAsync(HttpContext.Session.GetUserId());
                    var remainingAmounts = new Dictionary<int, decimal>();
                    if (customer != null)
                    {
                        foreach (var purchase in (ViewBag.EligiblePolicies as List<InsuranceClaims.Models.Entities.PolicyPurchase>) ?? new())
                        {
                            var claims = await _policyRepo.GetClaimsByCustomerAndPolicyAsync(customer.CustomerId, purchase.PolicyId);
                            var lastRenewalDate = purchase.Renewals?.OrderByDescending(r => r.RenewalDate).FirstOrDefault()?.RenewalDate;
                            decimal totalSettled = claims.Where(c => c.SettlementLog != null && c.SettlementLog.SettlementStatus == Enums.SettlementStatus.PROCESSED && (!lastRenewalDate.HasValue || c.CreatedAt >= lastRenewalDate.Value)).Sum(c => c.SettlementLog!.SettlementAmount);
                            remainingAmounts[purchase.PolicyId] = (purchase.Policy?.CoverageAmount ?? 0) - totalSettled;
                        }
                    }
                    ViewBag.RemainingAmounts = remainingAmounts;
                    return View(model);
                }
                var (success, message) = await _claimService.CreateClaimAsync(model, HttpContext.Session.GetUserId());
                TempData[success ? "Success" : "Error"] = message;
                if (!success)
                {
                    ViewBag.EligiblePolicies = await _claimService.GetEligiblePoliciesForClaimAsync(HttpContext.Session.GetUserId());
                    var customer = await _customerRepo.GetByUserIdAsync(HttpContext.Session.GetUserId());
                    var remainingAmounts = new Dictionary<int, decimal>();
                    if (customer != null)
                    {
                        foreach (var purchase in (ViewBag.EligiblePolicies as List<InsuranceClaims.Models.Entities.PolicyPurchase>) ?? new())
                        {
                            var claims = await _policyRepo.GetClaimsByCustomerAndPolicyAsync(customer.CustomerId, purchase.PolicyId);
                            var lastRenewalDate = purchase.Renewals?.OrderByDescending(r => r.RenewalDate).FirstOrDefault()?.RenewalDate;
                            decimal totalSettled = claims.Where(c => c.SettlementLog != null && c.SettlementLog.SettlementStatus == Enums.SettlementStatus.PROCESSED && (!lastRenewalDate.HasValue || c.CreatedAt >= lastRenewalDate.Value)).Sum(c => c.SettlementLog!.SettlementAmount);
                            remainingAmounts[purchase.PolicyId] = (purchase.Policy?.CoverageAmount ?? 0) - totalSettled;
                        }
                    }
                    ViewBag.RemainingAmounts = remainingAmounts;
                    return View(model);
                }
                return RedirectToAction("MyClaims");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating claim: {ex.Message}";
                ViewBag.EligiblePolicies = await _claimService.GetEligiblePoliciesForClaimAsync(HttpContext.Session.GetUserId());
                ViewBag.RemainingAmounts = new Dictionary<int, decimal>();
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
