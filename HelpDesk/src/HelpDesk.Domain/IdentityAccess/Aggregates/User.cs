using System;
using System.Collections.Generic;
using HelpDesk.Domain.IdentityAccess.Events;
using HelpDesk.Domain.IdentityAccess.ValueObjects;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.IdentityAccess.Aggregates
{
    public sealed class User : AggregateRoot<int>
    {
        public UserName Name { get; private set; } = default!;
        public EmailAddress Email { get; private set; } = default!;
        public UserRole Role { get; private set; } = default!;

        private User()
        {
        }

        private User(
            int id,
            UserName name,
            EmailAddress email,
            UserRole role) : base(id)
        {
            Name = name;
            Email = email;
            Role = role;
        }

        public static User CreateNew(
            UserName name,
            EmailAddress email,
            UserRole role)
        {
            return new User(
                id: 0,
                name: name,
                email: email,
                role: role);
        }

        public static User Rehydrate(
            int id,
            UserName name,
            EmailAddress email,
            UserRole role)
        {
            return new User(
                id: id,
                name: name,
                email: email,
                role: role);
        }

        public void ReplaceName(UserName newName)
        {
            Name = newName;
        }

        public void ReplaceEmail(EmailAddress newEmail)
        {
            Email = newEmail;
        }

        public void ReplaceRole(UserRole newRole)
        {
            Role = newRole;
        }

        public void RaiseCreatedEvent(string actorUserName, DateTime now)
        {
            Raise(UserCreatedDomainEvent.Create(
                userId: Id,
                name: Name.Value,
                email: Email.Value,
                role: Role.Value,
                actorUserName: actorUserName,
                occurredAt: now));
        }

        public void RaiseUpdatedEvent(
            string actorUserName,
            IReadOnlyList<UserUpdatedChange> changes,
            DateTime now)
        {
            if (changes.Count == 0)
                return;

            Raise(UserUpdatedDomainEvent.Create(
                userId: Id,
                actorUserName: actorUserName,
                changes: changes,
                occurredAt: now));
        }

        public void RaiseDeletedEvent(string actorUserName, DateTime now)
        {
            Raise(UserDeletedDomainEvent.Create(
                userId: Id,
                name: Name.Value,
                email: Email.Value,
                role: Role.Value,
                actorUserName: actorUserName,
                occurredAt: now));
        }
    }
}