namespace AlmaVault.Models.Domains
{
    public class ContributionLedger
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }  // References ApplicationUser.Id (Identity String GUID)
        public decimal Amount { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public ApplicationUser? User { get; set; }
    }
}
