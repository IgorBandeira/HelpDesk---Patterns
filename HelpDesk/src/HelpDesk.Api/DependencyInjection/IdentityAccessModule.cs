using HelpDesk.Application.IdentityAccess.UseCases.CreateUser;
using HelpDesk.Application.IdentityAccess.UseCases.DeleteUser;
using HelpDesk.Application.IdentityAccess.UseCases.GetUserById;
using HelpDesk.Application.IdentityAccess.UseCases.ListUsers;
using HelpDesk.Application.IdentityAccess.UseCases.PatchUser;

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

            return services;
        }
    }
}
