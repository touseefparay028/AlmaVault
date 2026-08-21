using System;
using System.ComponentModel.DataAnnotations;
using AlmaVault.Models.Domains;

namespace AlmaVault.Models.ViewModel
{
    public class CreateCampaignVM
    {
        [Required(ErrorMessage = "Campaign title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Campaign description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a campaign type.")]
        public CampaignType Type { get; set; }

        [Required(ErrorMessage = "Target amount is required.")]
        [Range(100, 10000000, ErrorMessage = "Target amount must be between $100 and $10,000,000.")]
        public decimal TargetAmount { get; set; }

        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
        public string BeneficiaryDepartment { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
    }
}