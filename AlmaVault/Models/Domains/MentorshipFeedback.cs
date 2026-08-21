using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaVault.Models.Domains
{
    public class MentorshipFeedback
    {
        [Key]
        public Guid Id { get; set; } 

        // Associated Mentorship Request (One feedback per engagement)
        [Required]
        public Guid MentorshipRequestId { get; set; }

        [ForeignKey(nameof(MentorshipRequestId))]
        public virtual MentorshipRequestsDM? MentorshipRequest { get; set; }

        // Rating from 1 to 5 Stars
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        // Optional Review Comment or Thank-You Note
        [StringLength(1000, ErrorMessage = "Feedback comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }

        // Author (Mentee or Mentor giving feedback)
        [Required]
        public Guid SubmittedById { get; set; }

        [ForeignKey(nameof(SubmittedById))]
        public virtual ApplicationUser? SubmittedBy { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }

}
