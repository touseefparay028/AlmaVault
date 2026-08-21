using System.ComponentModel.DataAnnotations;

namespace AlmaVault.Models.ViewModel
{
    public class CampaignDirectoryVM
    {
        public IEnumerable<ContributionCampaignVM> ActiveCampaigns { get; set; } = new List<ContributionCampaignVM>();
        public DonorImpactSummaryVM DonorSummary { get; set; } = new();
    }

    public class ContributionCampaignVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public int ProgressPercentage => TargetAmount > 0
            ? (int)Math.Min(100, Math.Round((CurrentAmount / TargetAmount) * 100))
            : 0;
        public string BeneficiaryDepartment { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }
    }

    public class DonateViewModel
    {
        public Guid CampaignId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;

        [Required, Range(1, 1000000, ErrorMessage = "Please enter a valid amount.")]
        public decimal Amount { get; set; }

        public string ContributionType { get; set; } = "MonetaryOneTime";
        public string? EquipmentDetails { get; set; }
        public string PaymentMethod { get; set; } = "Card";
    }

    public class DonorImpactSummaryVM
    {
        public decimal TotalLifetimeGiving { get; set; }
        public int CampaignsSupportedCount { get; set; }
        public string TierName { get; set; } = "Contributor";
        public string BadgeCssClass { get; set; } = "bg-secondary";
        public List<ContributionHistoryVM> History { get; set; } = new();
    }

    public class ContributionHistoryVM
    {
        public Guid Id { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string? TaxReceiptPath { get; set; }
    }
}
