using System;
using System.Collections.Generic;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.IdentityAccess.Events
{
    public sealed record UserUpdatedDomainEvent(
        int UserId,
        string ActorUserName,
        IReadOnlyList<UserUpdatedChange> Changes,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static UserUpdatedDomainEvent Create(
            int userId,
            string actorUserName,
            IReadOnlyList<UserUpdatedChange> changes,
            DateTime occurredAt)
            => new(
                userId,
                actorUserName,
                changes,
                Guid.NewGuid(),
                occurredAt);
    }

    public sealed record UserUpdatedChange(string Description);
}