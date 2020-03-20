using Domain.Tenants.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Domain.Tenants.Data
{
    public class TenantsDbContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; set; }

        public TenantsDbContext(DbContextOptions<TenantsDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Taken from here: https://medium.com/oppr/net-core-using-entity-framework-core-in-a-separate-project-e8636f9dc9e5
        /// </summary>
        public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TenantsDbContext>
        {
            public TenantsDbContext CreateDbContext(string[] args)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(@Directory.GetCurrentDirectory() + "/../Domain.Tenants/tenantsconfig.json")
                    .Build();

                var builder = new DbContextOptionsBuilder<TenantsDbContext>();
                var connectionString = configuration.GetConnectionString("TenantsSimple");
                builder.UseSqlServer(connectionString);
                return new TenantsDbContext(builder.Options);
            }
        }
    }
}
