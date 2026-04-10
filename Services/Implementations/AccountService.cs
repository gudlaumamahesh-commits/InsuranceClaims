using InsuranceClaims.Enums;
using InsuranceClaims.Helpers;
using InsuranceClaims.Models.Entities;
using InsuranceClaims.Models.ViewModels;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;

namespace InsuranceClaims.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly UserRepository     _userRepo;
        private readonly CustomerRepository _customerRepo;

        public AccountService(UserRepository userRepo, CustomerRepository customerRepo)
        {
            _userRepo     = userRepo;
            _customerRepo = customerRepo;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            // Enforce lowercase email for customers
            var email = model.Email.ToLowerInvariant().Trim();

            // Customer email must be @gmail.com
            if (!email.EndsWith("@gmail.com"))
                return (false, "Customer email must be a Gmail address (e.g., yourname@gmail.com).");

            var existing = await _userRepo.GetByEmailAsync(email);
            if (existing != null) return (false, "Email is already registered.");

            PasswordHelper.CreatePasswordHash(model.Password, out var hash, out var salt);

            var user = new User
            {
                Email        = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role         = UserRole.Customer
            };
            await _userRepo.AddAsync(user);

            var customer = new Customer
            {
                UserId = user.UserId,
                Name   = model.Name,
                Phone  = model.Phone
            };
            await _customerRepo.AddAsync(customer);

            return (true, "Registration successful. Please login.");
        }

        public async Task<User?> LoginAsync(LoginViewModel model)
        {
            // Normalize email to lowercase before lookup
            var email = model.Email.ToLowerInvariant().Trim();
            var user  = await _userRepo.GetByEmailAsync(email);
            if (user == null) return null;

            // Check if Officer or Surveyor is inactive
            if (user.Role == UserRole.Officer && user.Officer != null && !user.Officer.IsActive)
                return null; // Inactive officers cannot login
            if (user.Role == UserRole.Surveyor && user.Surveyor != null && !user.Surveyor.IsActive)
                return null; // Inactive surveyors cannot login

            if (!PasswordHelper.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt))
                return null;
            return user;
        }
    }
}
