using Domain.Tenants.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Domain.Tenants.Controllers
{
    [ApiController]
    [Route("api/tenants")]
    public class TenantsController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TenantsController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("GetTenantName")]
        public IActionResult GetTenantName()
        {
            var tenantName = _httpContextAccessor.HttpContext.GetTenant().Name.ToLowerInvariant();
            return Ok(tenantName);
        }
    }
}
