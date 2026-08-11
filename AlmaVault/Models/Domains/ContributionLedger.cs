namespace AlmaVault.Models.Domains
{
    public class ContributionLedger
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty; // References ApplicationUser.Id (Identity String GUID)
        public decimal Amount { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public ApplicationUser? User { get; set; }
    }
}
