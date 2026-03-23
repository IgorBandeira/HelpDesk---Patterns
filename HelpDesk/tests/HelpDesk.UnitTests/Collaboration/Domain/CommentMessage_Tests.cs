using FluentAssertions;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.UnitTests.Collaboration.Domain
{
    public class CommentMessage_Tests
    {
        [Fact]
        public void Create_Should_Require_Trim_And_Enforce_MaxLength()
        {
            Action a1 = () => CommentMessage.Create(null);
            a1.Should().Throw<DomainException>();

            Action a2 = () => CommentMessage.Create("   ");
            a2.Should().Throw<DomainException>();

            CommentMessage.Create("  hi  ").Value.Should().Be("hi");

            var tooLong = new string('a', CommentMessage.MaxChars + 1);
            Action a3 = () => CommentMessage.Create(tooLong);
            a3.Should().Throw<DomainException>();
        }
    }
}
