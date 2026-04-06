using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.IdentityAccess.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class UserUpdatedDomainEventHandler
        : IDomainEventHandler<UserUpdatedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public UserUpdatedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(UserUpdatedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var data = new Dictionary<string, string>
            {
                ["UserId"] = domainEvent.UserId.ToString(),
                ["ActorUserName"] = domainEvent.ActorUserName,
                ["OccurredAt"] = domainEvent.OccurredAt.ToString("O"),
                ["Changes"] = string.Join("||", domainEvent.Changes.Select(x => x.Description))
            };

            await _notify.NotifyManagersAsync("UserUpdated", data, ct);
        }
    }
}