using HelpDesk.Application.Operations.Ports;
using HelpDesk.Infrastructure.Operations.Notifications.Templates;
using HelpDesk.Infrastructure.Operations.Queries;
using HelpDesk.Infrastructure.Ticketing.HostedServices;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Operations.DependencyInjection
{
    public static class NotificationInfrastructureModule
    {
        public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<TicketEmailTemplateBuilder>();
            services.AddScoped<UserEmailTemplateBuilder>();
            services.AddScoped<CategoryEmailTemplateBuilder>();
            services.AddScoped<INotificationPort, NotificationPort>();
            services.AddHostedService<TicketSlaMonitorHostedService>();


            return services;
        }
    }
}