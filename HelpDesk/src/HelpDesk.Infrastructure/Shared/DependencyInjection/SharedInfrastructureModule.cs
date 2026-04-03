using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Infrastructure.Shared.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Shared.DependencyInjection
{
    public static class SharedInfrastructureModule
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IClock, SystemClock>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            return services;
        }
    }
}