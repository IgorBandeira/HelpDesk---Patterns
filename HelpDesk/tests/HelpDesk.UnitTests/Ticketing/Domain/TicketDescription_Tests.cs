using FluentAssertions;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.Ticketing.ValueObjects;

namespace HelpDesk.UnitTests.Ticketing.Domain
{
    public class TicketDescription_Tests
    {
        [Fact]
        public void Create_Should_Require_And_Trim()
        {
            Action a1 = () => TicketDescription.Create(null);
            a1.Should().Throw<DomainException>();

            Action a2 = () => TicketDescription.Create("   ");
            a2.Should().Throw<DomainException>();

            TicketDescription.Create("  Desc  ").Value.Should().Be("Desc");
        }
    }
}
