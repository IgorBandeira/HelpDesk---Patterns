using HelpDesk.Application.Operations.EventHandlers;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Ticketing.UseCases.AssignTicket;
using HelpDesk.Application.Ticketing.UseCases.CancelTicket;
using HelpDesk.Application.Ticketing.UseCases.ChangeRequester;
using HelpDesk.Application.Ticketing.UseCases.ChangeStatus;
using HelpDesk.Application.Ticketing.UseCases.CreateTicket;
using HelpDesk.Application.Ticketing.UseCases.GetTicketById;
using HelpDesk.Application.Ticketing.UseCases.ReopenTicket;
using HelpDesk.Application.Ticketing.UseCases.UpdateTicket;
using HelpDesk.Domain.Ticketing.Events;
using HelpDesk.Infrastructure.Shared.DomainEvents;

namespace HelpDesk.Api.DependencyInjection
{
    public static class TicketingModule
    {
        public static IServiceCollection AddTicketingApi(this IServiceCollection services)
        {
            services.AddScoped<CreateTicketHandler>();
            services.AddScoped<GetTicketByIdHandler>();
            services.AddScoped<ListTicketsHandler>();
            services.AddScoped<UpdateTicketHandler>();
            services.AddScoped<AssignTicketHandler>();
            services.AddScoped<ChangeRequesterHandler>();
            services.AddScoped<ChangeStatusHandler>();
            services.AddScoped<ReopenTicketHandler>();
            services.AddScoped<CancelTicketHandler>();

            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            services.AddScoped<IDomainEventHandler<TicketCreatedDomainEvent>, TicketCreatedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<TicketAssignedDomainEvent>, TicketAssignedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<TicketRequesterChangedDomainEvent>, TicketRequesterChangedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<TicketStatusChangedDomainEvent>, TicketStatusChangedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<TicketCanceledDomainEvent>, TicketCanceledDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<TicketReopenedDomainEvent>, TicketReopenedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<TicketUpdatedDomainEvent>, TicketUpdatedDomainEventHandler>();

            return services;
        }
    }
}