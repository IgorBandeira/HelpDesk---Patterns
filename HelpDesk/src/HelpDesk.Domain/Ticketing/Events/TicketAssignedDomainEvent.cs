using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Ticketing.Events
{
    public sealed record TicketAssignedDomainEvent(
        int TicketId,
        int AgentId,
        string AgentName,
        string PerformedByUserName,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static TicketAssignedDomainEvent Create(
            int ticketId,
            int agentId,
            string agentName,
            string performedByUserName,
            DateTime occurredAt)
            => new(ticketId, agentId, agentName, performedByUserName, Guid.NewGuid(), occurredAt);
    }
}