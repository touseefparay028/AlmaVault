using System.ComponentModel.DataAnnotations;

namespace AlmaVault.Models.ViewModel
{
    public class FeedBackVM
    {
        public Guid Id { get; set; }

        public int MentorshipRequestId { get; set; }

        [Required(ErrorMessage = "Please select a star rating.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Feedback comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }

        public string? SubmittedByName { get; set; }

        public DateTime SubmittedAt { get; set; }
    }
}
