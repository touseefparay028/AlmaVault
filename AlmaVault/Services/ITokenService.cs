using AlmaVault.Models.Domains;

namespace AlmaVault.Services
{
   

        public interface ITokenService
        {
            Task<string> GenerateJwtTokenAsync(ApplicationUser user);
        }
    
}
