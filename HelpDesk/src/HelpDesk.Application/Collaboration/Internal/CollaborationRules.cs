using HelpDesk.Domain.Ticketing.Enums;

namespace HelpDesk.Application.Collaboration.Internal
{
    internal static class CollaborationRules
    {
        internal static bool IsManager(string role)
            => string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase);

        internal static bool Participants(int userId, string userRole, int? requesterId, int? assigneeId)
            => IsManager(userRole) || userId == requesterId || userId == assigneeId;

        internal static bool TicketIsInactive(string status)
            => status is TicketStatus.Fechado or TicketStatus.Cancelado;
    }
}
