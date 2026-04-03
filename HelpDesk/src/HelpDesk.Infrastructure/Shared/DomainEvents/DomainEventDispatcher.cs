using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.SharedKernel.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Shared.DomainEvents
{
    public sealed class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainEventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken ct = default)
        {
            foreach (var domainEvent in domainEvents)
            {
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                var handlers = _serviceProvider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    var method = handlerType.GetMethod(nameof(IDomainEventHandler<DomainEvent>.HandleAsync));
                    if (method is null)
                        continue;

                    var task = (Task?)method.Invoke(handler, new object[] { domainEvent, ct });
                    if (task is not null)
                        await task;
                }
            }
        }
    }
}