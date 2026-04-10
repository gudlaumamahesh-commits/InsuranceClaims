using System.ComponentModel.DataAnnotations;

namespace InsuranceClaims.Models.ViewModels
{
    public class AddOfficerViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).{6,}$",
            ErrorMessage = "Password must be at least 6 characters with 1 uppercase letter and 1 number.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain alphabets only.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian phone number.")]
        public string Phone { get; set; } = string.Empty;
    }
}
