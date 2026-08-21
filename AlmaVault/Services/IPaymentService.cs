using AlmaVault.Services;
using AlmaVault.Models.ViewModel;

namespace AlmaVault.Services
{
    public class PaymentProcessingResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public interface IPaymentService
    {
        Task<PaymentProcessingResult> ProcessPaymentAsync(DonateViewModel model, string donorEmail);
    }
}