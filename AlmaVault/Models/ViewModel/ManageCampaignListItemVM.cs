using System;
using AlmaVault.Models.Domains;

namespace AlmaVault.Models.ViewModel
{
    public class ManageCampaignListItemVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public CampaignType Type { get; set; }
        public string BeneficiaryDepartment { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalDonorsCount { get; set; }

        public int ProgressPercentage => TargetAmount > 0
            ? (int)Math.Min(100, Math.Round((CurrentAmount / TargetAmount) * 100))
            : 0;
    }
}