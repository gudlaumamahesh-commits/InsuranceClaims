using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;

namespace InsuranceClaims.Services.Implementations
{
    public class FraudService : IFraudService
    {
        private readonly FraudRepository _fraudRepo;
        private readonly ClaimRepository _claimRepo;

        public FraudService(FraudRepository fraudRepo, ClaimRepository claimRepo)
        {
            _fraudRepo = fraudRepo;
            _claimRepo = claimRepo;
        }

        public async Task<List<Claim>> GetClaimsForFraudCheckAsync()
            => await _fraudRepo.GetClaimsReadyForFraudCheckAsync();

        public async Task<(bool Success, string Message)> SubmitFraudCheckAsync(FraudCheckViewModel model)
        {
            var claim = await _claimRepo.GetByIdAsync(model.ClaimId);
            if (claim == null) return (false, $"Claim #{model.ClaimId} not found.");

            var existing = await _fraudRepo.GetByClaimIdAsync(model.ClaimId);
            if (existing != null) return (false, $"Fraud check already done for Claim #{model.ClaimId}.");

            var fc = new FraudCheck
            {
                ClaimId    = model.ClaimId,
                FraudScore = model.FraudScore,
                RiskFlag   = model.RiskFlag
            };
            await _fraudRepo.AddAsync(fc);

            string guidance = model.RiskFlag switch
            {
                RiskFlag.LOW    => "LOW risk — claim is ready for approval.",
                RiskFlag.MEDIUM => "MEDIUM risk — review carefully before approving.",
                RiskFlag.HIGH   => "HIGH risk — suspected fraud. Investigate before any decision.",
                _               => ""
            };

            await _claimRepo.AddTrackingAsync(new ClaimTracking
            {
                ClaimId   = model.ClaimId,
                Status    = "FRAUD_CHECKED",
                Remarks   = $"Score: {model.FraudScore}/100, Risk: {model.RiskFlag}. {guidance}",
                UpdatedAt = DateTime.Now
            });

            return (true, $"Fraud check submitted. Risk: {model.RiskFlag} (Score {model.FraudScore}/100). {guidance}");
        }
    }
}
