using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Multitenancy;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleMultiTenant.Security
{
    public class InCurrentTenantRequirement : AuthorizationHandler<InCurrentTenantRequirement>, IAuthorizationRequirement
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InCurrentTenantRequirement(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InCurrentTenantRequirement requirement)
        {
            var tenantName = _httpContextAccessor.HttpContext.GetTenant().Name;

            if (context.User.FindFirstValue("tid") == tenantName)
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}
