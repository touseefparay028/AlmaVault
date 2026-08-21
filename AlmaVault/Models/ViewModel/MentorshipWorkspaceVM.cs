using AlmaVault.ViewModels;

namespace AlmaVault.Models.ViewModel
{
    public class MentorshipWorkspaceVM
    {
        public Guid RequestId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = "Approved";

        // Partner / Opposing User Details
        public string PartnerId { get; set; } = string.Empty;

        public string PartnerName { get; set; } = string.Empty;

        public string PartnerEmail { get; set; } = string.Empty;

        public string PartnerRole { get; set; } = string.Empty; // "Mentor" or "Mentee"

        // Context check for current user
        public bool IsUserMentor { get; set; }

        // Workspace Components
        public List<NoteItemViewModel> Notes { get; set; } = new();

        public List<TaskItemVM> Tasks { get; set; } = new();

        public FeedBackVM? ExistingFeedback { get; set; }

        // Calculated Helpers
        public int CompletedTasksCount => Tasks.Count(t => t.IsCompleted);

        public int TotalTasksCount => Tasks.Count;
    }
}
