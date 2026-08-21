namespace AlmaVault.Models.Domains
{
    public class MentorshipSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Topic { get; set; } = string.Empty;
        public Guid MentorId { get; set; }// References ApplicationUser.Id
        public DateTime ScheduledAt { get; set; }

        // Navigation Property
        public ApplicationUser? Mentor { get; set; }
    }
}
