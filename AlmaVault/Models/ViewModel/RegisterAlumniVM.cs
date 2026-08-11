using System.ComponentModel.DataAnnotations;

namespace AlmaVault.Models.ViewModel
{
    public class RegisterAlumniVM
    {
        [Required]
        public string RollNumber { get; set; } = string.Empty;

        [Required]
        public int GraduationYear { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? CurrentCompany { get; set; }
        public string? Designation { get; set; }
        public string? Location { get; set; }

        [Required]
        public IFormFile ProofDocument { get; set; } = null!;
    }
}
