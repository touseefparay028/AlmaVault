using System.ComponentModel.DataAnnotations;

namespace AlmaVault.Models.ViewModel
{
    public class CreateJobPostingVM
    {
        [Required]
        public Guid AlumniId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Company { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string ReferralContactEmail { get; set; } = string.Empty;
    }
}
