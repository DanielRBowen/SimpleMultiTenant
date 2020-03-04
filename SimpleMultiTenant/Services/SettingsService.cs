using Domain.Tenants.Multitenancy;
using Microsoft.AspNetCore.Http;
using SimpleMultiTenant.Data;
using System.Linq;

namespace SimpleMultiTenant.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SettingsService(ApplicationDbContext applicationDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _applicationDbContext = applicationDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public string DisplayHasGoods
        {
            get
            {
                return $"{_httpContextAccessor.HttpContext.GetTenant().Name}, has goods:{_applicationDbContext.Goods.Any().ToString()}";
            }
        }

    }
}
