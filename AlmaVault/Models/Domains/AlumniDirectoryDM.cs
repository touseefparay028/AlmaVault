using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaVault.Models.Domains
{
    public class AlumniDirectoryDM
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign key to ApplicationUser (AspNetUsers)
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        public string Designation { get; set; } = string.Empty;
        public string CurrentCompany { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        // Privacy Controls
        public bool IsEmailPublic { get; set; } = true;
        public bool IsPhonePublic { get; set; } = false;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}