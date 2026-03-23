using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Domain.Ticketing.Enums;

namespace HelpDesk.Application.Ticketing.Services
{
    public static class TicketSlaPolicy
    {
        public static bool ShouldAlert(SlaMonitorTicketDto ticket, DateTime now)
        {
            if (ticket.SlaDueAt is null)
                return false;

            if (string.Equals(ticket.Status, TicketStatus.Fechado.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ticket.Status, TicketStatus.Cancelado.ToString(), StringComparison.OrdinalIgnoreCase))
                return false;

            var start = ticket.SlaStartAt;
            var due = ticket.SlaDueAt.Value;

            if (due <= now)
                return false;

            var total = (due - start).TotalMinutes;
            if (total <= 0)
                return false;

            var elapsed = (now - start).TotalMinutes;
            return elapsed / total >= 0.85;
        }
    }
}