using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Infrastructure.Attachments.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Attachments.DependencyInjection
{
    public static class AttachmentsInfrastructureModule
    {
        public static IServiceCollection AddAttachmentsInfrastructure(this IServiceCollection services)
        {

            services.AddScoped<IAttachmentRepository, AttachmentRepository>();

            return services;
        }
    }
}