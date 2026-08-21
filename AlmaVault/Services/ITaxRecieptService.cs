using AlmaVault.Models.Domains;


namespace AlmaVault.Services
{
    public interface ITaxReceiptService
    {
        Task<string> GeneratePdfReceiptAsync(AlumniContribution contribution, string donorName, string donorEmail);
    }
}