using HelpDesk.Domain.Ticketing.Enums;

namespace HelpDesk.Application.Attachments.Internal
{
    internal static class AttachmentTicketRules
    {
        internal static bool IsInactive(string status)
            => status is TicketStatus.Fechado or TicketStatus.Cancelado;
    }
}
