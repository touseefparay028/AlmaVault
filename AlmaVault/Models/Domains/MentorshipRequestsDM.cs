using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaVault.Models.Domains
{
    public class MentorshipRequestsDM
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Note { get; set; }

        // Foreign Key for Mentee (Student)
        [Required]
        public string MenteeId { get; set; } = string.Empty;

        [ForeignKey(nameof(MenteeId))]
        public virtual ApplicationUser? Mentee { get; set; }

        // Foreign Key for Mentor (Alumni)
        [Required]
        public string MentorId { get; set; } = string.Empty;

        [ForeignKey(nameof(MentorId))]
        public virtual ApplicationUser? Mentor { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected", "Cancelled"

        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedDate { get; set; }
    }
}
