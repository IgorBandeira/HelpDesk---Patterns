using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Ticketing.Events
{
    public sealed record TicketStatusChangedDomainEvent(
        int TicketId,
        string PreviousStatus,
        string NewStatus,
        string PerformedByUserName,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static TicketStatusChangedDomainEvent Create(
            int ticketId,
            string previousStatus,
            string newStatus,
            string performedByUserName,
            DateTime occurredAt)
            => new(
                ticketId,
                previousStatus,
                newStatus,
                performedByUserName,
                Guid.NewGuid(),
                occurredAt);
    }
}