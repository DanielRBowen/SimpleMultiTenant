using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Domain.Tenants.Multitenancy
{
    /// <summary>
    /// Resolve the host to a tenant identifier
    /// </summary>
    public class HostResolutionStrategy : ITenantResolutionStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public HostResolutionStrategy(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        /// <summary>
        /// Get the tenant identifier
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<(string domainName, string ipAddresss, string name)> GetTenantIdentifierAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return await Task.FromResult((string.Empty, string.Empty, string.Empty));
            }
            else
            {
                var domainName = httpContext.Request.Host.Host;
                var ipAddress = GetIpAddress(httpContext);

                if (_configuration.GetValue<bool>("UsePathToResolveTenant"))
                {
                    var path = await Task.FromResult(httpContext.Request.Path);

                    if (path.HasValue)
                    {
                        var tenantIdentifier = path.Value.Split('/')[1];
                        return await Task.FromResult((domainName, ipAddress, tenantIdentifier));
                    }
                    else
                    {
                        return await Task.FromResult((domainName, ipAddress, string.Empty));
                    }
                }
                else
                {
                    return await Task.FromResult((domainName, ipAddress, GetSubDomain(httpContext)));
                }
            }
        }

        private static string GetIpAddress(HttpContext httpContext)
        {
            var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            var ipAddressString = remoteIpAddress.ToString();

            if (remoteIpAddress.IsIPv4MappedToIPv6)
            {
                return remoteIpAddress.MapToIPv6().ToString();
            }
            else
            {
                return remoteIpAddress.MapToIPv4().ToString();
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/38549143/how-do-i-get-the-current-subdomain-within-net-core-middleware
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        private static string GetSubDomain(HttpContext httpContext)
        {
            var subDomain = string.Empty;

            var host = httpContext.Request.Host.Host;

            if (!string.IsNullOrWhiteSpace(host))
            {
                subDomain = host.Split('.')[0];
            }

            return subDomain.Trim().ToLower();
        }
    }
}
