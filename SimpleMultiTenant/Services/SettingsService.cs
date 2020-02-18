using Microsoft.AspNetCore.Http;
using Multitenancy;
using SimpleMultiTenant.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
