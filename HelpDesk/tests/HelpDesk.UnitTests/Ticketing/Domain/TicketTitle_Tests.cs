using FluentAssertions;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.Ticketing.ValueObjects;

namespace HelpDesk.UnitTests.Ticketing.Domain
{
    public class TicketTitle_Tests
    {
        [Fact]
        public void Create_Should_Require_Trim_And_Enforce_MaxLength()
        {
            Action a1 = () => TicketTitle.Create(null);
            a1.Should().Throw<DomainException>();

            Action a2 = () => TicketTitle.Create("   ");
            a2.Should().Throw<DomainException>();

            TicketTitle.Create("  Hello  ").Value.Should().Be("Hello");

            var tooLong = new string('a', TicketTitle.MaxLength + 1);
            Action a3 = () => TicketTitle.Create(tooLong);
            a3.Should().Throw<DomainException>();
        }
    }
}
