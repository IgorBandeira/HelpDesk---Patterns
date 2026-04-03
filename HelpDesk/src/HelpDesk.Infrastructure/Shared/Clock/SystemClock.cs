using HelpDesk.Application.Shared.Abstractions;

namespace HelpDesk.Infrastructure.Shared.Clock
{
    public sealed class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
    }
}