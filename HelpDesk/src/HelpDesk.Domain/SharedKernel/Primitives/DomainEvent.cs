namespace HelpDesk.Domain.SharedKernel.Primitives
{
    public abstract record DomainEvent(Guid EventId, DateTime OccurredAt);
}
