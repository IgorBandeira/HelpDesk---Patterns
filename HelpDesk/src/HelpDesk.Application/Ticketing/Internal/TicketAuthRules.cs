using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Domain.Ticketing.Aggregates;

namespace HelpDesk.Application.Ticketing.Internal
{
    internal static class TicketAuthRules
    {
        internal static bool IsManager(UserSnapshot u) =>
            string.Equals(u.Role, "Manager", StringComparison.OrdinalIgnoreCase);

        internal static bool IsRequester(UserSnapshot u) =>
            string.Equals(u.Role, "Requester", StringComparison.OrdinalIgnoreCase);

        internal static bool IsAgent(UserSnapshot u) =>
            string.Equals(u.Role, "Agent", StringComparison.OrdinalIgnoreCase);

        internal static bool Owner(UserSnapshot u, Ticket t) =>
            IsManager(u) || (IsRequester(u) && u.Id == t.RequesterId);

        internal static bool Participants(UserSnapshot u, Ticket t) =>
            IsManager(u) || u.Id == t.RequesterId || (t.AssigneeId.HasValue && u.Id == t.AssigneeId.Value);
    }
}
