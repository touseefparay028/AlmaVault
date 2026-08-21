using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaVault.Models.Domains
{
    public class MentorshipNotes
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid MentorshipRequestId { get; set; }

        [ForeignKey(nameof(MentorshipRequestId))]
        public virtual MentorshipRequestsDM? MentorshipRequest { get; set; }

        [Required]
        public Guid AuthorId { get; set; } 

        [ForeignKey(nameof(AuthorId))]
        public virtual ApplicationUser? Author { get; set; }

        // PDF Attachment fields
        public string? FilePath { get; set; }
        public string? OriginalFileName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
