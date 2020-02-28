using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
namespace SimpleMultiTenant.Attributes
{
    //https://andrewlock.net/injecting-services-into-validationattributes-in-asp-net-core/
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class IsUserSignedInAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var userManager = (UserManager<IdentityUser>)validationContext
                         .GetService(typeof(UserManager<IdentityUser>));

            var httpContextAccessor = (IHttpContextAccessor)validationContext
                         .GetService(typeof(IHttpContextAccessor));

            var user = userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
            if (user == null)
            {
                return new ValidationResult("The user is not signed in.");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}
