using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Multitenancy;
using SimpleMultiTenant.Extensions;

namespace SimpleMultiTenant.Services
{
    public class SettingsCache : ISettingsCache
    {
        private readonly ILogger<SettingsCache> _logger;
        private readonly IDistributedCache _distributedCache;
        private readonly string _tenantName;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SettingsCache(
            ILogger<SettingsCache> logger,
            IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _tenantName = $"{_httpContextAccessor.HttpContext.GetTenant().Name}";
        }

        public void SetSetting<T>(object setting)
        {
            var settingName = typeof(T).Name;
            _distributedCache.Set($"{_tenantName}{settingName}", setting.ToByteArray());
        }

        public T GetSetting<T>()
        {
            var settingName = typeof(T).Name;
            return _distributedCache.Get($"{_tenantName}{settingName}").FromByteArray<T>();
        }

        public void ClearSetting<T>()
        {
            var settingName = typeof(T).Name;
            _distributedCache.Remove($"{_tenantName}{settingName}");
        }
    }
}
