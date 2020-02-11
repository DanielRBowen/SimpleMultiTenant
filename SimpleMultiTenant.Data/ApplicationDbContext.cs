using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SimpleMultiTenant.Data.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace SimpleMultiTenant.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Good> Goods { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// https://stackoverflow.com/questions/48117961/the-instance-of-entity-type-cannot-be-tracked-because-another-instance-of-th
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var type = entity.GetType();
            var et = this.Model.FindEntityType(type);
            var key = et.FindPrimaryKey();

            var keys = new object[key.Properties.Count];
            var x = 0;
            foreach (var keyName in key.Properties)
            {
                var keyProperty = type.GetProperty(keyName.Name, BindingFlags.Public | BindingFlags.Instance);
                keys[x++] = keyProperty.GetValue(entity);
            }

            var originalEntity = Find(type, keys);
            if (Entry(originalEntity).State == EntityState.Modified)
            {
                return base.Update(entity);
            }

            Entry(originalEntity).CurrentValues.SetValues(entity);
            return Entry((TEntity)originalEntity);
        }

        /// <summary>
        /// Taken from here: https://medium.com/oppr/net-core-using-entity-framework-core-in-a-separate-project-e8636f9dc9e5
        /// </summary>
        public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
        {
            public ApplicationDbContext CreateDbContext(string[] args)
            {

                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
//#if DEBUG
                    .AddJsonFile(@Directory.GetCurrentDirectory() + "/../SimpleMultiTenant/appsettings.json")
//#else
//                    .AddJsonFile(@Directory.GetCurrentDirectory() + "/wwwroot/custom/connection.json")
//#endif
                    .Build();

                var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
                var connectionString = configuration.GetConnectionString("Tenant1");
                builder.UseSqlServer(connectionString);
                return new ApplicationDbContext(builder.Options);
            }
        }
    }
}
