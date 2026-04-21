using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;

namespace InsuranceClaims.Services.Implementations
{
    public class SettlementService : ISettlementService
    {
        private readonly SettlementRepository _settlementRepo;
        private readonly ClaimRepository      _claimRepo;
        private readonly PolicyRepository     _policyRepo;

        public SettlementService(SettlementRepository settlementRepo, ClaimRepository claimRepo, PolicyRepository policyRepo)
        {
            _settlementRepo = settlementRepo;
            _claimRepo      = claimRepo;
            _policyRepo     = policyRepo;
        }

        public async Task<List<Claim>> GetClaimsForSettlementAsync()
            => await _settlementRepo.GetApprovedClaimsWithoutSettlementAsync();

        public async Task<List<Claim>> GetClaimsReadyForAdminAsync()
            => await _settlementRepo.GetClaimsReadyForAdminActionAsync();

        public async Task<(bool Success, string Message)> ProcessSettlementAsync(SettlementViewModel model)
        {
            try
            {
                var claim = await _claimRepo.GetByIdAsync(model.ClaimId);
                if (claim == null)
                    return (false, $"Claim #{model.ClaimId} not found.");
                if (claim.ClaimStatus != ClaimStatus.APPROVED)
                    return (false, $"Claim #{model.ClaimId} must be APPROVED before settlement. Current status: {claim.ClaimStatus}.");

                var existing = await _settlementRepo.GetByClaimIdAsync(model.ClaimId);
                if (existing != null)
                    return (false, $"Settlement already processed for Claim #{model.ClaimId}.");

                // Validate settlement amount doesn't exceed remaining coverage
                var policy = claim.Policy;
                if (policy != null)
                {
                    // Get all purchases for this customer and policy
                    var purchases = await _policyRepo.GetPurchasesByCustomerAsync(claim.CustomerId);
                    var purchase = purchases.FirstOrDefault(p => p.PolicyId == claim.PolicyId);
                    
                    if (purchase != null)
                    {
                        // Get the last renewal date
                        var lastRenewalDate = purchase.Renewals?.OrderByDescending(r => r.RenewalDate).FirstOrDefault()?.RenewalDate;
                        
                        // Calculate total settled claims after last renewal
                        var existingClaims = await _policyRepo.GetClaimsByCustomerAndPolicyAsync(claim.CustomerId, claim.PolicyId);
                        decimal totalSettled = existingClaims
                            .Where(c => c.SettlementLog != null && 
                                        c.SettlementLog.SettlementStatus == SettlementStatus.PROCESSED &&
                                        (!lastRenewalDate.HasValue || c.CreatedAt >= lastRenewalDate.Value))
                            .Sum(c => c.SettlementLog!.SettlementAmount);
                        
                        decimal remainingAmount = policy.CoverageAmount - totalSettled;
                        
                        if (model.SettlementAmount > remainingAmount)
                            return (false, $"Settlement amount ₹{model.SettlementAmount:N0} exceeds the remaining coverage of ₹{remainingAmount:N0}. Maximum allowed: ₹{remainingAmount:N0}.");
                    }
                }

                var log = new SettlementLog
                {
                    ClaimId          = model.ClaimId,
                    SettlementAmount = model.SettlementAmount,
                    SettlementStatus = SettlementStatus.PROCESSED,
                    Remarks          = model.Remarks,
                    ProcessedAt      = DateTime.Now
                };
                await _settlementRepo.AddAsync(log);

                string bankDetails = "";
                if (!string.IsNullOrEmpty(claim.BankName) && !string.IsNullOrEmpty(claim.BankAccountNumber))
                {
                    bankDetails = $" to {claim.BankName}, Account: {claim.BankAccountNumber}, IFSC: {claim.IFSCCode}";
                }

                await _claimRepo.AddTrackingAsync(new ClaimTracking
                {
                    ClaimId   = model.ClaimId,
                    Status    = "SETTLEMENT_PROCESSED",
                    Remarks   = $"Payment of ₹{model.SettlementAmount:N0} successfully transferred{bankDetails}. {model.Remarks}",
                    UpdatedAt = DateTime.Now
                });

                return (true, $"Settlement of ₹{model.SettlementAmount:N0} processed for Claim #{model.ClaimId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettlementService.ProcessSettlementAsync] {ex.Message}");
                return (false, "An error occurred while processing settlement. Please try again.");
            }
        }

        public async Task<(bool Success, string Message)> ApproveClaimAsync(int claimId, IClaimService claimService)
        {
            try
            {
                var claim = await _claimRepo.GetByIdAsync(claimId);
                if (claim == null) return (false, $"Claim #{claimId} not found.");
                if (claim.FraudCheck == null)
                    return (false, $"Claim #{claimId} has not been fraud-checked yet. Officer must run fraud check first.");

                // Get the assessed amount from surveyor
                decimal assessedAmount = claim.ClaimAmount;
                if (claim.Assessments.Any())
                {
                    var latestAssessment = claim.Assessments.OrderByDescending(a => a.AssessedAt).First();
                    assessedAmount = latestAssessment.AssessedAmount;
                }

                // Validate assessed amount doesn't exceed remaining coverage
                var policy = claim.Policy;
                if (policy != null)
                {
                    // Get all purchases for this customer and policy
                    var purchases = await _policyRepo.GetPurchasesByCustomerAsync(claim.CustomerId);
                    var purchase = purchases.FirstOrDefault(p => p.PolicyId == claim.PolicyId);
                    
                    if (purchase != null)
                    {
                        // Get the last renewal date
                        var lastRenewalDate = purchase.Renewals?.OrderByDescending(r => r.RenewalDate).FirstOrDefault()?.RenewalDate;
                        
                        // Calculate total settled claims after last renewal
                        var existingClaims = await _policyRepo.GetClaimsByCustomerAndPolicyAsync(claim.CustomerId, claim.PolicyId);
                        decimal totalSettled = existingClaims
                            .Where(c => c.SettlementLog != null && 
                                        c.SettlementLog.SettlementStatus == SettlementStatus.PROCESSED &&
                                        (!lastRenewalDate.HasValue || c.CreatedAt >= lastRenewalDate.Value))
                            .Sum(c => c.SettlementLog!.SettlementAmount);
                        
                        decimal remainingAmount = policy.CoverageAmount - totalSettled;
                        
                        if (assessedAmount > remainingAmount)
                            return (false, $"Cannot approve! Surveyor assessed amount ₹{assessedAmount:N0} exceeds the remaining coverage of ₹{remainingAmount:N0}. The surveyor has approved more than the available balance. Please contact the surveyor to reassess the claim or reject this claim.");
                    }
                }

                await claimService.UpdateClaimStatusAsync(claimId, ClaimStatus.APPROVED,
                    $"Claim approved by Admin. Fraud score: {claim.FraudCheck.FraudScore}/100, Risk: {claim.FraudCheck.RiskFlag}.");
                return (true, $"Claim #{claimId} approved successfully. It will now appear in Settlement.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettlementService.ApproveClaimAsync] {ex.Message}");
                return (false, "An error occurred while approving the claim. Please try again.");
            }
        }

        public async Task<(bool Success, string Message)> RejectClaimAsync(int claimId, string remarks, IClaimService claimService)
        {
            try
            {
                var claim = await _claimRepo.GetByIdAsync(claimId);
                if (claim == null) return (false, $"Claim #{claimId} not found.");

                await claimService.UpdateClaimStatusAsync(claimId, ClaimStatus.REJECTED,
                    $"Claim rejected by Admin. Reason: {remarks}");
                return (true, $"Claim #{claimId} rejected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettlementService.RejectClaimAsync] {ex.Message}");
                return (false, "An error occurred while rejecting the claim. Please try again.");
            }
        }
    }
}
