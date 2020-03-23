using Domain.Tenants.Data;
using Domain.Tenants.Multitenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleMultiTenant.Data;
using SimpleMultiTenant.FileManagement;
using SimpleMultiTenant.Security;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleMultiTenant.Areas.Admin.Api
{
    [ApiController]
    [Route("{tenant?}/api/[controller]")]
    [Route("api/[controller]")]
    [Authorize(Policy = PolicyNames.RequireTenant, Roles = "admin, mod")]
    public class ModController : ControllerBase
    {
        private readonly ILogger<ModController> _logger;
        private readonly TenantsDbContext _tenantsDbContext;
        private readonly IConfiguration _configuration;
        private readonly string _tenantsDirectory = Directory.GetCurrentDirectory() + "/wwwroot/tenants/";

        public ModController(
            ILogger<ModController> logger,
            TenantsDbContext tenantsDbContext,
            IConfiguration configuration)
        {
            _logger = logger;
            _tenantsDbContext = tenantsDbContext;
            _configuration = configuration;
        }

        [HttpGet("GetTenants")]
        public async Task<IActionResult> GetTenants()
        {
            try
            {
                await RemoveTenantsWithoutCustomFolders();
                return Ok(await _tenantsDbContext.Tenants.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost("CreateNewTenant")]
        public async Task<IActionResult> CreateNewTenant(Tenant tenant)
        {
            try
            {
                if (tenant == null || string.IsNullOrWhiteSpace(tenant.Name) || string.IsNullOrWhiteSpace(tenant.ConnectionString))
                {
                    return StatusCode((int)HttpStatusCode.UnprocessableEntity, "The tenant name and connection string must not be null");
                }

                var tenantName = tenant.Name;
                var connectionString = Regex.Unescape(tenant.ConnectionString);
                var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

                if (_tenantsDbContext.Tenants.Any(tenant => tenant.Name == tenantName))
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"The tenant: {tenantName}. Is already in the tenants database.");
                }

                var newTenant = new Tenant
                {
                    Guid = Guid.NewGuid().ToString(),
                    Name = tenantName,
                    ConnectionString = connectionStringBuilder.ConnectionString
                };

                _tenantsDbContext.Tenants.Add(newTenant);
                await _tenantsDbContext.SaveChangesAsync();

                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                var options = optionsBuilder.UseSqlServer(connectionStringBuilder.ConnectionString).Options;
                var dbContext = new ApplicationDbContext(options);
                dbContext.Database.Migrate();
                //Data.SeedData.Seed(dbContext);

                if (TenantsCustomFolderManager.CreateContentDirectoryIfItDoesNotExist(_tenantsDirectory, tenantName))
                {
                    _logger.LogInformation($"Custom content folder created for: {tenantName}");
                }

                TenantsCustomFolderManager.CreateTenantInConfiguration(tenantName, connectionStringBuilder.ConnectionString);
                return Ok("Tenant Created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete("DeleteTenant/{tenantName}")]
        public async Task<IActionResult> DeleteTenant(string tenantName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    return StatusCode((int)HttpStatusCode.UnprocessableEntity, "The tenant name cannot be empty");
                }

                if (_configuration.GetValue<string>("DefaultTenant") == tenantName)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "You cannot delete the default tenant");
                }

                var tenantToDelete = _tenantsDbContext.Tenants.FirstOrDefault(tenant => tenant.Name == tenantName);

                if (tenantToDelete != null)
                {
                    TenantsCustomFolderManager.DeleteTenantsCustomFolder(_tenantsDirectory, tenantName);
                    TenantsCustomFolderManager.DeleteTenantFromConfiguration(tenantToDelete.Name);
                    CustomTenantsFileManager.RemoveCustomTenant(tenantToDelete.Name);
                    _tenantsDbContext.Tenants.Remove(tenantToDelete);
                    await _tenantsDbContext.SaveChangesAsync();
                    return Ok($"Tenant: {tenantName} has been deleted");
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"The tenant: {tenantName}. Does not exist.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost("UpdateDomainNames")]
        public async Task<IActionResult> UpdateDomainNames(Tenant tenant)
        {
            try
            {
                if (tenant == null || string.IsNullOrWhiteSpace(tenant.Name))
                {
                    return StatusCode((int)HttpStatusCode.UnprocessableEntity, "The tenant name must not be null");
                }

                var tenantName = tenant.Name;

                if (_tenantsDbContext.Tenants.Any(tenant => tenant.Name == tenantName) == false)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"The tenant: {tenantName}. Is not in the database so the domain names could not be updated.");
                }

                var updatedTenant = _tenantsDbContext.Tenants.SingleOrDefault(tenant => tenant.Name == tenantName);

                if (updatedTenant.DomainNames != tenant.DomainNames)
                {
                    var oldDomainNames = updatedTenant.DomainNames;
                    updatedTenant.DomainNames = tenant.DomainNames;
                    _tenantsDbContext.Update(updatedTenant);
                    await _tenantsDbContext.SaveChangesAsync();
                    CustomTenantsFileManager.UpdateCustomTenant(updatedTenant);
                    return Ok($"Updated {tenantName}'s Domain Names from: {oldDomainNames} to: {tenant.DomainNames}");
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"The domain names for {tenantName} are the same.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost("UpdateIpAddresses")]
        public async Task<IActionResult> UpdateIpAddresses(Tenant tenant)
        {
            try
            {
                if (tenant == null || string.IsNullOrWhiteSpace(tenant.Name))
                {
                    return StatusCode((int)HttpStatusCode.UnprocessableEntity, "The tenant name must not be null");
                }

                var tenantName = tenant.Name;

                if (_tenantsDbContext.Tenants.Any(tenant => tenant.Name == tenantName) == false)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"The tenant: {tenantName}. Is not in the database so the ip addresses could not be updated.");
                }

                var updatedTenant = _tenantsDbContext.Tenants.SingleOrDefault(tenant => tenant.Name == tenantName);

                if (updatedTenant.IpAddresses != tenant.IpAddresses)
                {
                    var oldIpAddresses = updatedTenant.IpAddresses;
                    updatedTenant.IpAddresses = tenant.IpAddresses;
                    _tenantsDbContext.Update(updatedTenant);
                    await _tenantsDbContext.SaveChangesAsync();
                    CustomTenantsFileManager.UpdateCustomTenant(updatedTenant);
                    return Ok($"Updated {tenantName}'s Domain Names from: {oldIpAddresses} to: {tenant.IpAddresses}");
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"The ip addresses for {tenantName} are the same.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private async Task RemoveTenantsWithoutCustomFolders()
        {
            var tenants = await _tenantsDbContext.Tenants.ToListAsync();

            tenants.ForEach(tenant =>
            {
                if (TenantsCustomFolderManager.DoesTenantHaveCustomFolder(_tenantsDirectory, tenant.Name) == false)
                {
                    TenantsCustomFolderManager.DeleteTenantFromConfiguration(tenant.Name);
                    _tenantsDbContext.Remove(tenant);
                }
            });

            await _tenantsDbContext.SaveChangesAsync();
        }
    }
}
