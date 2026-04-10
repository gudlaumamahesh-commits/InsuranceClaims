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

        public AssessmentService(AssessmentRepository assessRepo, SurveyorRepository surveyorRepo,
            ClaimRepository claimRepo)
        {
            _assessRepo   = assessRepo;
            _surveyorRepo = surveyorRepo;
            _claimRepo    = claimRepo;
        }

        public async Task<List<SurveyorAssignment>> GetAssignedClaimsForSurveyorAsync(int surveyorId)
        {
            var all = await _assessRepo.GetAssignedClaimsForSurveyorAsync(surveyorId);

            // Filter out claims that already have an assessment submitted by this surveyor
            // (so the card disappears after assessment is done)
            return all.Where(a =>
                a.Claim != null &&
                !a.Claim.Assessments.Any(x => x.SurveyorId == surveyorId)
            ).ToList();
        }

        public async Task<(bool Success, string Message)> SubmitAssessmentAsync(AssessmentViewModel model, int userId)
        {
            var surveyor = await _surveyorRepo.GetByUserIdAsync(userId);
            if (surveyor == null) return (false, "Surveyor profile not found.");

            // Check claim exists
            var claim = await _claimRepo.GetByIdAsync(model.ClaimId);
            if (claim == null) return (false, $"Claim #{model.ClaimId} not found.");

            // Check not already assessed by this surveyor
            if (claim.Assessments.Any(a => a.SurveyorId == surveyor.SurveyorId))
                return (false, $"You have already submitted an assessment for Claim #{model.ClaimId}.");
            var coverageAmount = claim.Policy?.CoverageAmount??0;
			if (coverageAmount>0 && model.AssessedAmount > coverageAmount)
				return (false, $"Assessed amount ₹{model.AssessedAmount:N0} cannot exceed the policy Coverage of ₹{coverageAmount:N0}.");
			var assessment = new Assessment
            {
                ClaimId           = model.ClaimId,
                SurveyorId        = surveyor.SurveyorId,
                AssessedAmount    = model.AssessedAmount,
                AssessmentRemarks = model.AssessmentRemarks,
                AssessedAt        = DateTime.Now
            };
            await _assessRepo.AddAsync(assessment);

            // Add tracking - notifies officer
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
