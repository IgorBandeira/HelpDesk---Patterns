using HelpDesk.Application.IdentityAccess.UseCases.CreateUser;
using HelpDesk.Application.IdentityAccess.UseCases.DeleteUser;
using HelpDesk.Application.IdentityAccess.UseCases.GetUserById;
using HelpDesk.Application.IdentityAccess.UseCases.ListUsers;
using HelpDesk.Application.IdentityAccess.UseCases.PatchUser;
using HelpDesk.Application.Operations.EventHandlers;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.IdentityAccess.Events;

namespace HelpDesk.Api.DependencyInjection
{
    public static class IdentityAccessModule
    {
        public static IServiceCollection AddIdentityAccessApi(this IServiceCollection services)
        {
            services.AddScoped<CreateUserHandler>();
            services.AddScoped<GetUserByIdHandler>();
            services.AddScoped<ListUsersHandler>();
            services.AddScoped<PatchUserHandler>();
            services.AddScoped<DeleteUserHandler>();

            services.AddScoped<IDomainEventHandler<UserCreatedDomainEvent>, UserCreatedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<UserUpdatedDomainEvent>, UserUpdatedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<UserDeletedDomainEvent>, UserDeletedDomainEventHandler>();

            return services;
        }
    }
}