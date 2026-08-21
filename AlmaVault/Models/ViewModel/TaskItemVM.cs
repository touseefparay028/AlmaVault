namespace AlmaVault.Models.ViewModel
{
    public class TaskItemVM
    {
        public Guid Id { get; set; }

        public Guid MentorshipRequestId { get; set; }
        public string? Title { get; set; }

        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
