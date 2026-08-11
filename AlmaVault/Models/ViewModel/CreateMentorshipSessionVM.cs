using System.ComponentModel.DataAnnotations;

namespace AlmaVault.Models.ViewModel
{
    public class CreateMentorshipSessionVM
    {
        [Required]
        public int MentorAlumniId { get; set; }

        [Required]
        public string Topic { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledTime { get; set; }

        [Required, Url]
        public string MeetingUrl { get; set; } = string.Empty;
    }
}
