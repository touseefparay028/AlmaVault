namespace AlmaVault.Models.Domains
{
    public class MentorshipSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Topic { get; set; } = string.Empty;
        public string MentorId { get; set; } = string.Empty; // References ApplicationUser.Id
        public DateTime ScheduledAt { get; set; }

        // Navigation Property
        public ApplicationUser? Mentor { get; set; }
    }
}
