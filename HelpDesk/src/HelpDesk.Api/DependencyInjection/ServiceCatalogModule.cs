using HelpDesk.Application.Operations.EventHandlers;
using HelpDesk.Application.ServiceCatalog.UseCases.CreateCategory;
using HelpDesk.Application.ServiceCatalog.UseCases.DeleteCategory;
using HelpDesk.Application.ServiceCatalog.UseCases.GetCategoryById;
using HelpDesk.Application.ServiceCatalog.UseCases.ListCategories;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.ServiceCatalog.Events;

namespace HelpDesk.Api.DependencyInjection
{
    public static class ServiceCatalogModule
    {
        public static IServiceCollection AddServiceCatalogApi(this IServiceCollection services)
        {
            services.AddScoped<CreateCategoryHandler>();
            services.AddScoped<GetCategoryByIdHandler>();
            services.AddScoped<ListCategoriesHandler>();
            services.AddScoped<DeleteCategoryHandler>();

            services.AddScoped<IDomainEventHandler<CategoryCreatedDomainEvent>, CategoryCreatedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<CategoryDeletedDomainEvent>, CategoryDeletedDomainEventHandler>();

            return services;
        }
    }
}