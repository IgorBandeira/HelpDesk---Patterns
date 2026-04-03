using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Ticketing.Events
{
    public sealed record TicketCanceledDomainEvent(
        int TicketId,
        int ActorUserId,
        string ActorUserName,
        string Reason,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static TicketCanceledDomainEvent Create(
            int ticketId,
            int actorUserId,
            string actorUserName,
            string reason,
            DateTime occurredAt)
            => new(ticketId, actorUserId, actorUserName, reason, Guid.NewGuid(), occurredAt);
    }
}