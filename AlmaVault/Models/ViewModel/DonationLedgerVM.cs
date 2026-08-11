namespace AlmaVault.Models.ViewModel
{
    public class DonationLedgerVM
    {
        public Guid TransactionId { get; set; }
        public string AlumniName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? CertificateUrl { get; set; }
    }
}
