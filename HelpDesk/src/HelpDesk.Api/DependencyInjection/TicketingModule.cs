using HelpDesk.Application.Ticketing.UseCases.AssignTicket;
using HelpDesk.Application.Ticketing.UseCases.CancelTicket;
using HelpDesk.Application.Ticketing.UseCases.ChangeRequester;
using HelpDesk.Application.Ticketing.UseCases.ChangeStatus;
using HelpDesk.Application.Ticketing.UseCases.CreateTicket;
using HelpDesk.Application.Ticketing.UseCases.GetTicketById;
using HelpDesk.Application.Ticketing.UseCases.ListTickets;
using HelpDesk.Application.Ticketing.UseCases.ReopenTicket;
using HelpDesk.Application.Ticketing.UseCases.UpdateTicket;

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

            return services;
        }
    }
}