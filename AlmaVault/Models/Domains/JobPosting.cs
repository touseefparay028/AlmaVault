using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaVault.Models.Domains
{
    public class JobPosting
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string EmploymentType { get; set; } = "Full-Time"; // Full-Time, Part-Time, Internship, Remote

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Requirements { get; set; } = string.Empty;

        public string ApplicationUrlOrEmail { get; set; } = string.Empty;

        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Foreign Key to ApplicationUser (Alumnus who created the post)
        [Required]
        public string PostedByUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(PostedByUserId))]
        public virtual ApplicationUser? PostedByUser { get; set; }
    }
}
