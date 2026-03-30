using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Ports;
using HelpDesk.Application.Operations.Ports;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Operations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HelpDesk.Infrastructure.Operations.Notifications.Templates;

namespace HelpDesk.Infrastructure.Operations.Queries
{
    public sealed class NotificationPort : INotificationPort
    {
        private readonly AppDbContext _db;
        private readonly IClock _clock;
        private readonly ILogger<NotificationPort> _logger;
        private readonly IEmailSender _email;
        private readonly TicketEmailTemplateBuilder _templateBuilder;

        public NotificationPort(
            AppDbContext db,
            IClock clock,
            ILogger<NotificationPort> logger,
            IEmailSender email,
            TicketEmailTemplateBuilder templateBuilder)
        {
            _db = db;
            _clock = clock;
            _logger = logger;
            _email = email;
            _templateBuilder = templateBuilder;
        }

        public async Task NotifyTicketActionAsync(
            int ticketId,
            string description,
            string? extraEmail = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Ticket #{Id} ação: {Desc}", ticketId, description);

            await SaveTicketActionAsync(ticketId, description, ct);

            var emails = await GetTicketParticipantEmailsAsync(ticketId, ct);

            if (!string.IsNullOrWhiteSpace(extraEmail))
                emails.Add(extraEmail);

            emails = emails
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (emails.Count == 0)
                return;

            var ticket = await _db.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == ticketId, ct);

            var categoryName = ticket is null
                ? null
                : await GetCategoryNameAsync(ticket.CategoryId, ct);

            var subject = _templateBuilder.BuildActionSubject(ticketId, description);
            var html = _templateBuilder.BuildActionEmail(ticketId, description, ticket, categoryName);

            await _email.SendAsync(
                new EmailMessage(emails, subject, html),
                ct);
        }

        public async Task NotifySlaAlertAsync(int ticketId, CancellationToken ct = default)
        {
            var ticket = await _db.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == ticketId, ct);

            if (ticket is null)
            {
                _logger.LogWarning("Ticket #{Id} não encontrado para alerta de SLA.", ticketId);
                return;
            }

            _logger.LogWarning("SLA alerta (≥85%): Ticket #{Id} vence em {Due}", ticket.Id, ticket.SlaDueAt);

            var emails = await GetTicketParticipantEmailsAsync(ticket.Id, ct);

            _logger.LogInformation(
                "Ticket #{Id} destinatários SLA: {Emails}",
                ticket.Id,
                emails.Count == 0 ? "(nenhum)" : string.Join(", ", emails));

            if (emails.Count == 0)
                return;

            var categoryName = await GetCategoryNameAsync(ticket.CategoryId, ct);

            var subject = _templateBuilder.BuildSlaAlertSubject(ticket.Id);
            var html = _templateBuilder.BuildSlaAlertEmail(ticket, categoryName);

            await _email.SendAsync(
                new EmailMessage(emails, subject, html),
                ct);
        }

        private async Task SaveTicketActionAsync(int ticketId, string description, CancellationToken ct)
        {
            var action = new TicketActionEntity
            {
                TicketId = ticketId,
                Description = description,
                CreatedAt = _clock.Now
            };

            _db.TicketActions.Add(action);
            await _db.SaveChangesAsync(ct);
        }

        private async Task<List<string>> GetTicketParticipantEmailsAsync(int ticketId, CancellationToken ct)
        {
            var projection = await _db.Tickets
                .AsNoTracking()
                .Where(x => x.Id == ticketId)
                .Select(x => new
                {
                    RequesterEmail = _db.Users
                        .Where(u => u.Id == x.RequesterId)
                        .Select(u => u.Email)
                        .FirstOrDefault(),

                    AssigneeEmail = x.AssigneeId.HasValue
                        ? _db.Users
                            .Where(u => u.Id == x.AssigneeId.Value)
                            .Select(u => u.Email)
                            .FirstOrDefault()
                        : null
                })
                .FirstOrDefaultAsync(ct);

            var all = new List<string?>();

            if (projection is not null)
            {
                all.Add(projection.RequesterEmail);
                all.Add(projection.AssigneeEmail);
            }

            return all
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()!;
        }

        private async Task<string?> GetCategoryNameAsync(int? categoryId, CancellationToken ct)
        {
            if (categoryId is null)
                return null;

            return await _db.Categories
                .AsNoTracking()
                .Where(c => c.Id == categoryId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(ct);
        }
    }
}