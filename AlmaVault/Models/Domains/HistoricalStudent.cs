using System.ComponentModel.DataAnnotations;

namespace AlmaVault.Models.Domains
{
    public class HistoricalStudent
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        public int GraduationYear { get; set; }

        public string StudentIdNumber { get; set; } = string.Empty;

        public DateTime ConvertedToAlumniDate { get; set; } = DateTime.UtcNow;

        public string ConvertedByAdminEmail { get; set; } = string.Empty;
    }
}
