using FluentAssertions;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.Ticketing.Enums;

namespace HelpDesk.UnitTests.Ticketing.Domain
{
    public class TicketPriority_Tests
    {
        [Fact]
        public void EnsureValid_Should_Allow_Only_Known_Values()
        {
            Action a1 = () => TicketPriority.EnsureValid(null!);
            a1.Should().Throw<DomainException>();

            Action a2 = () => TicketPriority.EnsureValid("Invalid");
            a2.Should().Throw<DomainException>();

            Action ok = () => TicketPriority.EnsureValid(TicketPriority.Media);
            ok.Should().NotThrow();
        }

        [Fact]
        public void ToSla_Should_Map_To_Hours()
        {
            TicketPriority.ToSla(TicketPriority.Critica).TotalHours.Should().Be(8);
            TicketPriority.ToSla(TicketPriority.Alta).TotalHours.Should().Be(24);
            TicketPriority.ToSla(TicketPriority.Media).TotalHours.Should().Be(48);
            TicketPriority.ToSla(TicketPriority.Baixa).TotalHours.Should().Be(72);
        }
    }
}
