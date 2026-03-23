using HelpDesk.Application.Collaboration.Ports;
using HelpDesk.Infrastructure.Collaboration.Queries;
using HelpDesk.Infrastructure.Collaboration.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Collaboration.DependencyInjection
{
    public static class CollaborationInfrastructureModule
    {
        public static IServiceCollection AddCollaborationInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICommentReadPort, CommentReadPort>();

            return services;
        }
    }
}