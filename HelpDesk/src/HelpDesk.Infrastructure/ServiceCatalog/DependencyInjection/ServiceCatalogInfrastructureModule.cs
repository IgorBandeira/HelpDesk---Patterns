using HelpDesk.Application.ServiceCatalog.Ports;
using HelpDesk.Infrastructure.ServiceCatalog.Queries;
using HelpDesk.Infrastructure.ServiceCatalog.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.ServiceCatalog.DependencyInjection
{
    public static class ServiceCatalogInfrastructureModule
    {
        public static IServiceCollection AddServiceCatalogInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryReadPort, CategoryReadPort>();

            return services;
        }
    }
}
