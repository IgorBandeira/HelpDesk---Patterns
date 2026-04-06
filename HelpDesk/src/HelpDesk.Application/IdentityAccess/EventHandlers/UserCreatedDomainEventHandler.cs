using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.IdentityAccess.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class UserCreatedDomainEventHandler
        : IDomainEventHandler<UserCreatedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public UserCreatedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(UserCreatedDomainEvent domainEvent, CancellationToken ct = default)
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

            await _notify.NotifyManagersAsync("UserCreated", data, ct);
        }
    }
}