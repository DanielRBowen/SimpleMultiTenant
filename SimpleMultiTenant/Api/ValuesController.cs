using Domain.Tenants.Multitenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleMultiTenant.Data;
using System;
using System.Threading.Tasks;

namespace SimpleMultiTenant.Api
{
    public class OperationIdService
    {
        public readonly Guid Id;

        public OperationIdService()
        {
            Id = Guid.NewGuid();
        }
    }

    [Route("/api/{tenant?}/values")]
    public class ValuesController : Controller
    {
        private readonly OperationIdService _operationIdService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _applicationDbContext;

        public ValuesController(OperationIdService operationIdService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext applicationDbContext)
        {
            _operationIdService = operationIdService;
            _httpContextAccessor = httpContextAccessor;
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet(""), AllowAnonymous]
        public async Task<ActionResult> GetOperationIdValue([FromRoute]string tenant)
        {
            var currentTenant = _httpContextAccessor.HttpContext.GetTenant();
            return Ok(_operationIdService.Id);
        }
    }
}
