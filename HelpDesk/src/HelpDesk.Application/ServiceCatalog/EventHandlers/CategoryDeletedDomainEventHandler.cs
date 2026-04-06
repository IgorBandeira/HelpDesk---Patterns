using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.ServiceCatalog.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.ServiceCatalog.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class CategoryDeletedDomainEventHandler
        : IDomainEventHandler<CategoryDeletedDomainEvent>
    {
        private readonly INotificationPort _notify;
        private readonly ICategoryReadPort _categories;

        public CategoryDeletedDomainEventHandler(
            INotificationPort notify,
            ICategoryReadPort categories)
        {
            _notify = notify;
            _categories = categories;
        }

        public async Task HandleAsync(CategoryDeletedDomainEvent domainEvent, CancellationToken ct = default)
        {
            string parentName = string.Empty;

            if (domainEvent.ParentId.HasValue)
            {
                parentName = await _categories.GetNameAsync(domainEvent.ParentId.Value) ?? string.Empty;
            }

            var data = new Dictionary<string, string>
            {
                ["CategoryId"] = domainEvent.CategoryId.ToString(),
                ["Name"] = domainEvent.Name,
                ["ParentName"] = parentName,
                ["ActorUserName"] = domainEvent.ActorUserName,
                ["OccurredAt"] = domainEvent.OccurredAt.ToString("O")
            };

            await _notify.NotifyManagersAsync("CategoryDeleted", data, ct);
        }
    }
}