using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;

namespace InsuranceClaims.Services.Interfaces
{
    public interface IFraudService
    {
        Task<List<Claim>> GetClaimsForFraudCheckAsync();
        Task<(bool Success, string Message)> SubmitFraudCheckAsync(FraudCheckViewModel model);
    }
}
