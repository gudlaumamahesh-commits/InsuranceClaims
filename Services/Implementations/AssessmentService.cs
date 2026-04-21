using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;

namespace InsuranceClaims.Services.Implementations
{
    public class AssessmentService : IAssessmentService
    {
        private readonly AssessmentRepository _assessRepo;
        private readonly SurveyorRepository   _surveyorRepo;
        private readonly ClaimRepository      _claimRepo;
        private readonly PolicyRepository     _policyRepo;

        public AssessmentService(AssessmentRepository assessRepo, SurveyorRepository surveyorRepo,
            ClaimRepository claimRepo, PolicyRepository policyRepo)
        {
            _assessRepo   = assessRepo;
            _surveyorRepo = surveyorRepo;
            _claimRepo    = claimRepo;
            _policyRepo   = policyRepo;
        }

        public async Task<List<SurveyorAssignment>> GetAssignedClaimsForSurveyorAsync(int surveyorId)
        {
            var all = await _assessRepo.GetAssignedClaimsForSurveyorAsync(surveyorId);

            return all.Where(a =>
                a.Claim != null &&
                !a.Claim.Assessments.Any(x => x.SurveyorId == surveyorId)
            ).ToList();
        }

        public async Task<(bool Success, string Message)> SubmitAssessmentAsync(AssessmentViewModel model, int userId)
        {
            var surveyor = await _surveyorRepo.GetByUserIdAsync(userId);
            if (surveyor == null) return (false, "Surveyor profile not found.");

            var claim = await _claimRepo.GetByIdAsync(model.ClaimId);
            if (claim == null) return (false, $"Claim #{model.ClaimId} not found.");

            if (claim.Assessments.Any(a => a.SurveyorId == surveyor.SurveyorId))
                return (false, $"You have already submitted an assessment for Claim #{model.ClaimId}.");
            
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
                    
                    if (model.AssessedAmount > remainingAmount)
                        return (false, $"Assessed amount ₹{model.AssessedAmount:N0} exceeds the remaining coverage of ₹{remainingAmount:N0}. The customer has already claimed ₹{totalSettled:N0} from this policy. You can only assess up to ₹{remainingAmount:N0}.");
                }
            }

            var assessment = new Assessment
            {
                ClaimId           = model.ClaimId,
                SurveyorId        = surveyor.SurveyorId,
                AssessedAmount    = model.AssessedAmount,
                AssessmentRemarks = model.AssessmentRemarks,
                AssessedAt        = DateTime.Now
            };
            await _assessRepo.AddAsync(assessment);

            await _claimRepo.AddTrackingAsync(new ClaimTracking
            {
                ClaimId   = model.ClaimId,
                Status    = "ASSESSMENT_SUBMITTED",
                Remarks   = $"Surveyor {surveyor.Name} assessed ₹{model.AssessedAmount:N0}. " +
                            $"Remarks: {model.AssessmentRemarks ?? "None"}. " +
                            $"Officer can now run Fraud Check.",
                UpdatedAt = DateTime.Now
            });

            return (true, $"Assessment submitted successfully. Claim #{model.ClaimId} is now ready for Officer's Fraud Check.");
        }
    }
}
