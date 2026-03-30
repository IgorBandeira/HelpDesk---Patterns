using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Application.Ticketing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelpDesk.Infrastructure.Ticketing.HostedServices
{
    public sealed class TicketSlaMonitorHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TicketSlaMonitorHostedService> _logger;

        public TicketSlaMonitorHostedService(
            IServiceProvider serviceProvider,
            ILogger<TicketSlaMonitorHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var clock = scope.ServiceProvider.GetRequiredService<IClock>();
                    var query = scope.ServiceProvider.GetRequiredService<ITicketSlaQueryPort>();
                    var notify = scope.ServiceProvider.GetRequiredService<INotificationPort>();

                    var now = clock.Now;
                    var tickets = await query.ListOpenTicketsWithSlaAsync(stoppingToken);

                    foreach (var ticket in tickets)
                    {
                        try
                        {
                            var shouldAlert = TicketSlaPolicy.ShouldAlert(ticket, now);

                            if (!shouldAlert)
                                continue;

                            await notify.NotifySlaAlertAsync(ticket.Id, stoppingToken);

                            _logger.LogInformation(
                                "Notificação de SLA enviada para Ticket #{Id}",
                                ticket.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Erro ao processar alerta de SLA do Ticket #{Id}",
                                ticket.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no monitor de SLA dos chamados.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}