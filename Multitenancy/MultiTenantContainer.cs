using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Multitenancy
{
    internal class MultiTenantContainer<T> : IContainer where T : Tenant
    {
        //This is the base application container
        private readonly IContainer _applicationContainer;
        //This action configures a container builder
        private readonly Action<T, ContainerBuilder> _tenantContainerConfiguration;

        //This dictionary keeps track of all of the tenant scopes that we have created
        private readonly Dictionary<string, ILifetimeScope> _tenantLifetimeScopes = new Dictionary<string, ILifetimeScope>();

        private readonly object _lock = new object();
        private const string _multiTenantTag = "multitenantcontainer";

        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;

        public IDisposer Disposer => GetCurrentTenantScope().Disposer;

        public object Tag => GetCurrentTenantScope().Tag;

        public IComponentRegistry ComponentRegistry => GetCurrentTenantScope().ComponentRegistry;

        public MultiTenantContainer(IContainer applicationContainer, Action<T, ContainerBuilder> containerConfiguration)
        {
            _tenantContainerConfiguration = containerConfiguration;
            _applicationContainer = applicationContainer;
        }

        /// <summary>
        /// Get the current teanant from the application container
        /// </summary>
        /// <returns></returns>
        private T GetCurrentTenant()
        {
            //We have registered our TenantAccessService in Part 1, the service is available in the application container which allows us to access the current Tenant
            return _applicationContainer.Resolve<TenantAccessService<T>>().GetTenantAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get the scope of the current tenant
        /// </summary>
        /// <returns></returns>
        public ILifetimeScope GetCurrentTenantScope()
        {
            return GetTenantScope(GetCurrentTenant()?.Id);
        }

        /// <summary>
        /// Get (configure on missing)
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public ILifetimeScope GetTenantScope(string tenantId)
        {
            //If no tenant (e.g. early on in the pipeline, we just use the application container)
            if (tenantId == null)
                return _applicationContainer;

            //If we have created a lifetime for a tenant, return
            if (_tenantLifetimeScopes.ContainsKey(tenantId))
                return _tenantLifetimeScopes[tenantId];

            lock (_lock)
            {
                if (_tenantLifetimeScopes.ContainsKey(tenantId))
                {
                    return _tenantLifetimeScopes[tenantId];
                }
                else
                {
                    //This is a new tenant, configure a new lifetimescope for it using our tenant sensitive configuration method
                    _tenantLifetimeScopes.Add(tenantId, _applicationContainer.BeginLifetimeScope(_multiTenantTag, a => _tenantContainerConfiguration(GetCurrentTenant(), a)));
                    return _tenantLifetimeScopes[tenantId];
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var scope in _tenantLifetimeScopes)
                    scope.Value.Dispose();
                _applicationContainer.Dispose();
            }
        }

        public ILifetimeScope BeginLifetimeScope()
        {
            throw new NotImplementedException();
        }

        public ILifetimeScope BeginLifetimeScope(object tag)
        {
            return GetCurrentTenantScope().BeginLifetimeScope(tag);
        }

        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return GetCurrentTenantScope().BeginLifetimeScope(configurationAction);
        }

        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            return GetCurrentTenantScope().BeginLifetimeScope(tag, configurationAction);
        }

        public object ResolveComponent(ResolveRequest request)
        {
            return GetCurrentTenantScope().ResolveComponent(request);
        }

        public ValueTask DisposeAsync()
        {
            return GetCurrentTenantScope().DisposeAsync();
        }
    }
}
