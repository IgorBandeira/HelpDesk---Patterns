using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Ticketing.Events
{
    public sealed record TicketRequesterChangedDomainEvent(
        int TicketId,
        int RequesterId,
        string RequesterName,
        string PerformedByUserName,
        string? RequesterEmail,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static TicketRequesterChangedDomainEvent Create(
            int ticketId,
            int requesterId,
            string requesterName,
            string performedByUserName,
            string? requesterEmail,
            DateTime occurredAt)
            => new(
                ticketId,
                requesterId,
                requesterName,
                performedByUserName,
                requesterEmail,
                Guid.NewGuid(),
                occurredAt);
    }
}