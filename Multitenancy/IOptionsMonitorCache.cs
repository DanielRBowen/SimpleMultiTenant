using Microsoft.Extensions.Options;
using System;

namespace Multitenancy
{
    /// <summary>
    /// Tenant aware options cache
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="TTenant"></typeparam>
    public class TenantOptionsCache<TOptions, TTenant> : IOptionsMonitorCache<TOptions>
        where TOptions : class
        where TTenant : Tenant
    {

        private readonly ITenantAccessor<TTenant> _tenantAccessor;
        private readonly TenantOptionsCacheDictionary<TOptions> _tenantSpecificOptionsCache =
            new TenantOptionsCacheDictionary<TOptions>();

        public TenantOptionsCache(ITenantAccessor<TTenant> tenantAccessor)
        {
            _tenantAccessor = tenantAccessor;
        }

        public void Clear()
        {
            _tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Id).Clear();
        }

        public TOptions GetOrAdd(string name, Func<TOptions> createOptions)
        {
            return _tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Id)
                .GetOrAdd(name, createOptions);
        }

        public bool TryAdd(string name, TOptions options)
        {
            return _tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Id)
                .TryAdd(name, options);
        }

        public bool TryRemove(string name)
        {
            return _tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Id)
                .TryRemove(name);
        }
    }

}
