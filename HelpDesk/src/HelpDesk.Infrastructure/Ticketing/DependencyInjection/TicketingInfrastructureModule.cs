using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Infrastructure.Ticketing.Queries;
using HelpDesk.Infrastructure.Ticketing.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Ticketing.DependencyInjection
{
    public static class TicketingInfrastructureModule
    {
        public static IServiceCollection AddTicketingInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ITicketReadPort, TicketReadPort>();
            services.AddScoped<ITicketSlaQueryPort, TicketSlaQueryPort>();
            services.AddScoped<ITicketUserQueryPort, TicketUserQueryPort>();
            services.AddScoped<ITicketCategoryQueryPort, TicketCategoryQueryPort>();
            services.AddScoped<ITicketQueryPort, TicketQueryPort>();


            return services;
        }
    }
}