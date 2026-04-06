using System;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.IdentityAccess.Events
{
    public sealed record UserDeletedDomainEvent(
        int UserId,
        string Name,
        string Email,
        string Role,
        string ActorUserName,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static UserDeletedDomainEvent Create(
            int userId,
            string name,
            string email,
            string role,
            string actorUserName,
            DateTime occurredAt)
            => new(
                userId,
                name,
                email,
                role,
                actorUserName,
                Guid.NewGuid(),
                occurredAt);
    }
}