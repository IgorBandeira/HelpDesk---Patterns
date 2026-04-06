using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.IdentityAccess.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class UserDeletedDomainEventHandler
        : IDomainEventHandler<UserDeletedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public UserDeletedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(UserDeletedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var data = new Dictionary<string, string>
            {
                ["UserId"] = domainEvent.UserId.ToString(),
                ["Name"] = domainEvent.Name,
                ["Email"] = domainEvent.Email,
                ["Role"] = domainEvent.Role,
                ["ActorUserName"] = domainEvent.ActorUserName,
                ["OccurredAt"] = domainEvent.OccurredAt.ToString("O")
            };

            await _notify.NotifyManagersAsync("UserDeleted", data, ct);
        }
    }
}