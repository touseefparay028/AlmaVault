using System.ComponentModel.DataAnnotations;

namespace AlmaVault.Models.ViewModel
{
    public class CreateDonationVM
    {
        [Required]
        public int AlumniId { get; set; }

        [Required, Range(1, 10000000)]
        public decimal Amount { get; set; }

        [Required]
        public string CampaignName { get; set; } = string.Empty;
    }
}
