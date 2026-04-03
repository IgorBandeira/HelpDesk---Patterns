using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Ticketing.Events
{
    public sealed record TicketCreatedDomainEvent(
        int TicketId,
        int RequesterId,
        string RequesterName,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static TicketCreatedDomainEvent Create(
            int ticketId,
            int requesterId,
            string requesterName,
            DateTime occurredAt)
            => new(ticketId, requesterId, requesterName, Guid.NewGuid(), occurredAt);
    }
}