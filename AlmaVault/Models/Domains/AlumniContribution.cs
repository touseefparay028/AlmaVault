using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaVault.Models.Domains
{
    public class AlumniContribution
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        public ContributionCampaign? Campaign { get; set; }

        [Required]
        public Guid DonorId { get; set; }

        public ContributionType Type { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(250)]
        public string? ItemDescription { get; set; }

        [StringLength(100)]
        public string TransactionReference { get; set; } = string.Empty;

        public bool IsVerified { get; set; } = false;

        public string? TaxReceiptPath { get; set; }

        public DateTime ContributedAt { get; set; } = DateTime.UtcNow;
    }
}
