using Autofac;
using Microsoft.Extensions.Options;
using System;

namespace Multitenancy
{

    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Register tenant specific options
        /// </summary>
        /// <typeparam name="TOptions">Type of options we are apply configuration to</typeparam>
        /// <param name="tenantOptionsConfiguration">Action to configure options for a tenant</param>
        /// <returns></returns>
        public static ContainerBuilder RegisterTenantOptions<TOptions, T>(this ContainerBuilder builder, Action<TOptions, T> tenantConfig) where TOptions : class, new() where T : Tenant
        {
            builder.RegisterType<TenantOptionsCache<TOptions, T>>()
                .As<IOptionsMonitorCache<TOptions>>()
                .SingleInstance();

            builder.RegisterType<TenantOptionsFactory<TOptions, T>>()
                .As<IOptionsFactory<TOptions>>()
                .WithParameter(new TypedParameter(typeof(Action<TOptions, T>), tenantConfig))
                .SingleInstance();


            builder.RegisterType<TenantOptions<TOptions>>()
                .As<IOptionsSnapshot<TOptions>>()
                .SingleInstance();

            builder.RegisterType<TenantOptions<TOptions>>()
                .As<IOptions<TOptions>>()
                .SingleInstance();

            return builder;
        }
    }
}
