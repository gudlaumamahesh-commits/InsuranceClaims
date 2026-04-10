using System.ComponentModel.DataAnnotations;
using InsuranceClaims.Enums;

namespace InsuranceClaims.Models.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        [Required]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        [Required]
        public UserRole Role { get; set; }

        // Navigation
        public Customer? Customer { get; set; }
        public Officer? Officer { get; set; }
        public Surveyor? Surveyor { get; set; }
    }
}
