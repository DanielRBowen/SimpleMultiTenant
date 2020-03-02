using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Multitenancy;
using System;
namespace SimpleMultiTenant.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class IsUserInCurrentTenantAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            if (httpContext != null)
            {
                var signInManager = httpContext.RequestServices.GetService(typeof(SignInManager<IdentityUser>)) as SignInManager<IdentityUser>;
                var userClaimsPrinciple = httpContext.User;

                if (userClaimsPrinciple != null && signInManager.IsSignedIn(userClaimsPrinciple))
                {
                    var tenantId = httpContext.GetTenant().Id;
                    var tenantName = httpContext.GetTenant().Name;

                    if (userClaimsPrinciple.HasClaim("tid", tenantId) == false)
                    {
                        httpContext.Response.Redirect(new PathString($"/{tenantName}/Account/Login"));
                    }
                }
            }
        }
    }
}
