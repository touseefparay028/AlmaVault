using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaVault.Models.Domains
{
    public class MentorshipTasks
    {
        [Key]
        public Guid Id { get; set; }

        // Associated Mentorship Request
        [Required]
        public Guid MentorshipRequestId { get; set; }

        [ForeignKey(nameof(MentorshipRequestId))]
        public virtual MentorshipRequestsDM? MentorshipRequest { get; set; }

        // Task Details
        [Required]
        [StringLength(250, ErrorMessage = "Task description cannot exceed 250 characters.")]
        public string? Description { get; set; } 

        public bool IsCompleted { get; set; } = false;
        public DateTime DueDate { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }
}
