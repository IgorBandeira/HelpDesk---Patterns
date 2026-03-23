using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Infrastructure.Ticketing.HostedServices;
using HelpDesk.Infrastructure.Ticketing.Notifications.Templates;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Notifications.DependencyInjection
{
    public static class NotificationInfrastructureModule
    {
        public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<TicketEmailTemplateBuilder>();
            services.AddScoped<INotificationPort, NotificationPort>();
            services.AddHostedService<TicketSlaMonitorHostedService>();


            return services;
        }
    }
}