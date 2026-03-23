using HelpDesk.Application.Shared.Abstractions;

namespace HelpDesk.UnitTests.Shared.Fakes
{
    public sealed class FakeClock : IClock
    {
        public DateTime Now { get; set; } = new DateTime(2026, 1, 1, 10, 0, 0);
    }
}
