using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Multitenancy
{
    internal class MultitenantContainerMiddleware<T> where T : Tenant
    {
        private readonly RequestDelegate next;

        public MultitenantContainerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, Func<MultiTenantContainer<T>> multiTenantContainerAccessor)
        {
            //Set to current tenant container.
            //Begin new scope for request as ASP.NET Core standard scope is per-request
            context.RequestServices =
                new AutofacServiceProvider(multiTenantContainerAccessor()
                        .GetCurrentTenantScope().BeginLifetimeScope());
            await next.Invoke(context);
        }
    }
}
