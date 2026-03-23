using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Infrastructure.IdentityAccess.Queries;
using HelpDesk.Infrastructure.IdentityAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.IdentityAccess.DependencyInjection
{
    public static class IdentityAccessInfrastructureModule
    {
        public static IServiceCollection AddIdentityAccessInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserReadPort, UserReadPort>();

            return services;
        }
    }
}
