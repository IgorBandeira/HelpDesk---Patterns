using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.ServiceCatalog.Events
{
    public sealed record CategoryCreatedDomainEvent(
        int CategoryId,
        string Name,
        int? ParentId,
        string ActorUserName,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static CategoryCreatedDomainEvent Create(
            int categoryId,
            string name,
            int? parentId,
            string actorUserName,
            DateTime occurredAt)
            => new(
                categoryId,
                name,
                parentId,
                actorUserName,
                Guid.NewGuid(),
                occurredAt);
    }
}