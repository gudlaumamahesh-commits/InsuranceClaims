using InsuranceClaims.Enums;
using InsuranceClaims.Helpers;
using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;

namespace InsuranceClaims.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly UserRepository     _userRepo;
        private readonly OfficerRepository  _officerRepo;
        private readonly SurveyorRepository _surveyorRepo;

        public AdminService(UserRepository userRepo, OfficerRepository officerRepo, SurveyorRepository surveyorRepo)
        {
            _userRepo     = userRepo;
            _officerRepo  = officerRepo;
            _surveyorRepo = surveyorRepo;
        }

        public async Task<List<Officer>>  GetOfficersAsync()  => await _officerRepo.GetAllAsync();
        public async Task<List<Surveyor>> GetSurveyorsAsync() => await _surveyorRepo.GetAllAsync();

        public async Task<(bool Success, string Message)> AddOfficerAsync(AddOfficerViewModel model)
        {
            // Email must be lowercase and @insurance.com
            var email = model.Email.ToLowerInvariant().Trim();
            if (!email.EndsWith("@insurance.com"))
                return (false, "Officer email must end with @insurance.com (e.g., john@insurance.com).");

            if (await _userRepo.GetByEmailAsync(email) != null)
                return (false, "Email already exists.");

            PasswordHelper.CreatePasswordHash(model.Password, out var hash, out var salt);
            var user = new User { Email = email, PasswordHash = hash, PasswordSalt = salt, Role = UserRole.Officer };
            await _userRepo.AddAsync(user);
            await _officerRepo.AddAsync(new Officer { UserId = user.UserId, Name = model.Name, Phone = model.Phone, IsActive = true });
            return (true, $"Officer '{model.Name}' added successfully.");
        }

        public async Task<(bool Success, string Message)> AddSurveyorAsync(AddSurveyorViewModel model)
        {
            // Email must be lowercase and @insurance.com
            var email = model.Email.ToLowerInvariant().Trim();
            if (!email.EndsWith("@insurance.com"))
                return (false, "Surveyor email must end with @insurance.com (e.g., kumar@insurance.com).");

            if (await _userRepo.GetByEmailAsync(email) != null)
                return (false, "Email already exists.");

            PasswordHelper.CreatePasswordHash(model.Password, out var hash, out var salt);
            var user = new User { Email = email, PasswordHash = hash, PasswordSalt = salt, Role = UserRole.Surveyor };
            await _userRepo.AddAsync(user);
            await _surveyorRepo.AddAsync(new Surveyor { UserId = user.UserId, Name = model.Name, Phone = model.Phone, IsActive = true });
            return (true, $"Surveyor '{model.Name}' added successfully.");
        }

        public async Task<(bool Success, string Message)> ToggleOfficerActiveAsync(int officerId)
        {
            var (isNowActive, message) = await _officerRepo.ToggleActiveAsync(officerId);
            return (true, message);
        }

        public async Task<(bool Success, string Message)> ToggleSurveyorActiveAsync(int surveyorId)
        {
            var (isNowActive, message) = await _surveyorRepo.ToggleActiveAsync(surveyorId);
            return (true, message);
        }

        public async Task DeleteOfficerAsync(int officerId)   => await _officerRepo.DeleteAsync(officerId);
        public async Task DeleteSurveyorAsync(int surveyorId) => await _surveyorRepo.DeleteAsync(surveyorId);
    }
}
