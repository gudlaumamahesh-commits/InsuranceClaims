using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;

namespace InsuranceClaims.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);
        Task<User?> LoginAsync(LoginViewModel model);
    }
}
