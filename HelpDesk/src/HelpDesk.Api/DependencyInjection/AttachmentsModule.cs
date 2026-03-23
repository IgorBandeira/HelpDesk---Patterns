using HelpDesk.Application.Attachments.UseCases.DeleteAttachment;
using HelpDesk.Application.Attachments.UseCases.GetAttachmentById;
using HelpDesk.Application.Attachments.UseCases.ListAttachments;
using HelpDesk.Application.Attachments.UseCases.UploadAttachment;

namespace HelpDesk.Api.DependencyInjection
{
    public static class AttachmentsModule
    {
        public static IServiceCollection AddAttachmentsApi(this IServiceCollection services)
        {
            services.AddScoped<UploadAttachmentHandler>();
            services.AddScoped<ListAttachmentsHandler>();
            services.AddScoped<GetAttachmentByIdHandler>();
            services.AddScoped<DeleteAttachmentHandler>();

            return services;
        }
    }
}