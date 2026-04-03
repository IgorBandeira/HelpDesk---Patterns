using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Ticketing.Events
{
    public sealed record TicketUpdatedDomainEvent(
        int TicketId,
        IReadOnlyList<TicketUpdatedChange> Changes,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static TicketUpdatedDomainEvent Create(
            int ticketId,
            IReadOnlyList<TicketUpdatedChange> changes,
            DateTime occurredAt)
            => new(ticketId, changes, Guid.NewGuid(), occurredAt);
    }

    public sealed record TicketUpdatedChange(string Description);
}