using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaVault.Models.Domains
{
    public enum CampaignType
    {
        Infrastructure,
        Scholarship,
        Equipment,
        Research,
        General
    }

    public enum ContributionType
    {
        MonetaryOneTime,
        MonetaryRecurring,
        EquipmentGrant
    }

    public enum DonorTier
    {
        Contributor,
        Bronze,
        Silver,
        Gold,
        Benefactor
    }

}
