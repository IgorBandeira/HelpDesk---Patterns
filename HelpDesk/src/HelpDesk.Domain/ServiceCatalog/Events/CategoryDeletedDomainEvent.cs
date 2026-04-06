using System;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.ServiceCatalog.Events
{
    public sealed record CategoryDeletedDomainEvent(
        int CategoryId,
        string Name,
        int? ParentId,
        string ActorUserName,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static CategoryDeletedDomainEvent Create(
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