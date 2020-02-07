using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Multitenancy
{
    public class MultiTenantServiceProviderFactory<T> : IServiceProviderFactory<ContainerBuilder> where T : Tenant
    {

        private readonly Action<T, ContainerBuilder> _tenantSerivcesConfiguration;

        public MultiTenantServiceProviderFactory(Action<T, ContainerBuilder> tenantSerivcesConfiguration)
        {
            _tenantSerivcesConfiguration = tenantSerivcesConfiguration;
        }

        /// <summary>
        /// Create a builder populated with global services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public ContainerBuilder CreateBuilder(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            builder.Populate(services);

            return builder;
        }

        /// <summary>
        /// Create our serivce provider
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <returns></returns>
        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            MultiTenantContainer<T> container = null;

            Func<MultiTenantContainer<T>> containerAccessor = () =>
            {
                return container;
            };

            containerBuilder
                .RegisterInstance(containerAccessor)
                .SingleInstance();

            container = new MultiTenantContainer<T>(containerBuilder.Build(), _tenantSerivcesConfiguration);

            return new AutofacServiceProvider(containerAccessor());
        }
    }
}
