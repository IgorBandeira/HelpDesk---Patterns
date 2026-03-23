using HelpDesk.Application.Shared.Ports;
using HelpDesk.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Notifications.Email.DependencyInjection
{
    public static class EmailInfrastructureModule
    {
        public static IServiceCollection AddEmailInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<SmtpOptions>(
                configuration.GetSection("Smtp"));

            services.AddScoped<IEmailSender, MailKitEmailSender>();

            return services;
        }
    }
}