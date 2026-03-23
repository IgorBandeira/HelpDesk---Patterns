using HelpDesk.Application.Shared.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Shared.DependencyInjection
{
    public static class SharedInfrastructureModule
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IClock, SystemClock>();
            return services;
        }
    }
}