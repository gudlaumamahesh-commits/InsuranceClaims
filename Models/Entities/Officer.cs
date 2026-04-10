using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceClaims.Models.Entities
{
    public class Officer
    {
        [Key]
        public int OfficerId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain alphabets only.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian phone number.")]
        public string Phone { get; set; } = string.Empty;

        // Active / Inactive toggle — default Active
        public bool IsActive { get; set; } = true;

        public DateTime? DeactivatedAt { get; set; }
        public DateTime? ReactivatedAt { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}
