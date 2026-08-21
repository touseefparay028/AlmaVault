using Microsoft.AspNetCore.Identity;

namespace AlmaVault.Models.Domains
{
    public class ApplicationRole:IdentityRole<Guid>
    {
        public ApplicationRole() 
        { 
        }
    }
}
