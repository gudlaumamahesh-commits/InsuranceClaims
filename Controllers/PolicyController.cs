using InsuranceClaims.Enums;
using InsuranceClaims.Helpers;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceClaims.Controllers
{
    public class PolicyController : Controller
    {
        private readonly IPolicyService _policyService;
        private readonly ClaimRepository _claimRepo;
        private readonly CustomerRepository _customerRepo;

        public PolicyController(IPolicyService policyService, ClaimRepository claimRepo,
            CustomerRepository customerRepo)
        {
            _policyService  = policyService;
            _claimRepo      = claimRepo;
            _customerRepo   = customerRepo;
        }

        private bool IsCustomer() =>
            HttpContext.Session.IsLoggedIn() && HttpContext.Session.GetUserRole() == "Customer";

        public async Task<IActionResult> Buy(int id)
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Account");
            try
            {
                var policy = await _policyService.GetPolicyByIdAsync(id);
                if (policy == null) return NotFound();
                return View(policy);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading policy: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBuy(int policyId)
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Account");
            try
            {
                var (success, message) = await _policyService.BuyPolicyAsync(policyId, HttpContext.Session.GetUserId());
                TempData[success ? "Success" : "Error"] = message;
                return RedirectToAction("MyPolicies");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error purchasing policy: {ex.Message}";
                return RedirectToAction("MyPolicies");
            }
        }

        public async Task<IActionResult> MyPolicies()
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Account");
            try
            {
                var userId    = HttpContext.Session.GetUserId();
                var purchases = await _policyService.GetMyPoliciesAsync(userId);
                var customer = await _customerRepo.GetByUserIdAsync(userId);
                var allClaims = customer != null
                    ? await _claimRepo.GetByCustomerAsync(customer.CustomerId)
                    : new List<InsuranceClaims.Models.Entities.Claim>();

                var settledPolicies = new HashSet<int>();
                var claimsByPolicy  = new Dictionary<int, InsuranceClaims.Enums.ClaimStatus>();

                foreach (var c in allClaims)
                {
                    var fullClaim = await _claimRepo.GetByIdAsync(c.ClaimId);
                    if (fullClaim?.SettlementLog != null &&
                        fullClaim.SettlementLog.SettlementStatus == SettlementStatus.PROCESSED)
                    {
                        settledPolicies.Add(c.PolicyId);
                    }
                    if (!claimsByPolicy.ContainsKey(c.PolicyId))
                        claimsByPolicy[c.PolicyId] = c.ClaimStatus;
                }

                ViewBag.SettledPolicies = settledPolicies;
                ViewBag.ClaimsByPolicy  = claimsByPolicy;
                return View(purchases);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading policies: {ex.Message}";
                return View(new List<InsuranceClaims.Models.Entities.PolicyPurchase>());
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int purchaseId)
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Account");
            try
            {
                var (success, message) = await _policyService.RenewPolicyAsync(purchaseId);
                TempData[success ? "Success" : "Error"] = message;
                return RedirectToAction("MyPolicies");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error renewing policy: {ex.Message}";
                return RedirectToAction("MyPolicies");
            }
        }
    }
}
