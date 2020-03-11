using Domain.Tenants.Multitenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleMultiTenant.Services
{
    public class TenantClaimsSignInHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SignInManager<IdentityUser> _signInManager;

        public TenantClaimsSignInHelper(
            IHttpContextAccessor httpContextAccessor,
            SignInManager<IdentityUser> signInManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;
        }

        //
        // https://blog.dangl.me/archive/adding-custom-claims-when-logging-in-with-aspnet-core-identity-cookie/
        public async Task SignInUserAsync(IdentityUser user, bool isPersistent)
        {
            var customClaims = new[]
            {
                new Claim("tid", _httpContextAccessor.HttpContext.GetTenant().Guid)
            };

            await _signInManager.SignInWithClaimsAsync(user, isPersistent, customClaims);
        }

        /// <summary>
        ///  See: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task RefreshSignInAsync(IdentityUser user)
        {
            var auth = await _signInManager.Context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            var claims = new List<Claim>();
            var authenticationMethod = auth?.Principal?.FindFirst(ClaimTypes.AuthenticationMethod);
            if (authenticationMethod != null)
            {
                claims.Add(authenticationMethod);
            }
            var amr = auth?.Principal?.FindFirst("amr");
            if (amr != null)
            {
                claims.Add(amr);
            }

            claims.Add(new Claim("tid", _httpContextAccessor.HttpContext.GetTenant().Guid));
            await _signInManager.SignInWithClaimsAsync(user, auth?.Properties, claims);
        }
    }
}
