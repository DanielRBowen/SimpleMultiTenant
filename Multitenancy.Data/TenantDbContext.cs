using Microsoft.EntityFrameworkCore;
using System;

namespace Multitenancy.Data
{
    public class TenantDbContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; set; }

        public TenantDbContext(DbContextOptions<TenantDbContext> options)
           : base(options)
        {
        }
    }
}
