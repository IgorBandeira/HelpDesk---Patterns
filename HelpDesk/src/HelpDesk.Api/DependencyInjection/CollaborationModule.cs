using HelpDesk.Application.Collaboration.UseCases.AddComment;
using HelpDesk.Application.Collaboration.UseCases.DeleteComment;
using HelpDesk.Application.Collaboration.UseCases.GetCommentById;
using HelpDesk.Application.Collaboration.UseCases.ListComments;
using HelpDesk.Application.Collaboration.UseCases.ReplaceCommentMessage;

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

            return services;
        }
    }
}