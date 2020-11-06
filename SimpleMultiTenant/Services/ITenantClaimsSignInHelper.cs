using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace SimpleMultiTenant.Services
{
    public interface ITenantClaimsSignInHelper
    {
        Task RefreshSignInAsync(IdentityUser user);
        Task SignInUserAsync(IdentityUser user, bool isPersistent);
    }
}