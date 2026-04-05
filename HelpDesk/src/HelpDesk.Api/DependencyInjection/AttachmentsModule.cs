using HelpDesk.Application.Attachments.UseCases.DeleteAttachment;
using HelpDesk.Application.Attachments.UseCases.GetAttachmentById;
using HelpDesk.Application.Attachments.UseCases.ListAttachments;
using HelpDesk.Application.Attachments.UseCases.UploadAttachment;
using HelpDesk.Application.Operations.EventHandlers;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.Attachments.Events;

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

            services.AddScoped<IDomainEventHandler<AttachmentUploadedDomainEvent>, AttachmentUploadedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<AttachmentDeletedDomainEvent>, AttachmentDeletedDomainEventHandler>();

            return services;
        }
    }
}