using HelpDesk.Application.Collaboration.UseCases.AddComment;
using HelpDesk.Application.Collaboration.UseCases.DeleteComment;
using HelpDesk.Application.Collaboration.UseCases.GetCommentById;
using HelpDesk.Application.Collaboration.UseCases.ListComments;
using HelpDesk.Application.Collaboration.UseCases.ReplaceCommentMessage;
using HelpDesk.Application.Operations.EventHandlers;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.Collaboration.Events;

namespace HelpDesk.Api.DependencyInjection
{
    public static class CollaborationModule
    {
        public static IServiceCollection AddCollaborationApi(this IServiceCollection services)
        {
            services.AddScoped<AddCommentHandler>();
            services.AddScoped<GetCommentByIdHandler>();
            services.AddScoped<ListCommentsHandler>();
            services.AddScoped<ReplaceCommentMessageHandler>();
            services.AddScoped<DeleteCommentHandler>();

            services.AddScoped<IDomainEventHandler<CommentAddedDomainEvent>, CommentAddedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<CommentMessageReplacedDomainEvent>, CommentMessageReplacedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<CommentDeletedDomainEvent>, CommentDeletedDomainEventHandler>();

            return services;
        }
    }
}