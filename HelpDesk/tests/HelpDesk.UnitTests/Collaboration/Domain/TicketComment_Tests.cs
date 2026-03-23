using FluentAssertions;
using HelpDesk.Domain.Collaboration.Aggregates;
using HelpDesk.Domain.Collaboration.Enums;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.UnitTests.Collaboration.Domain
{
    public class TicketComment_Tests
    {
        [Fact]
        public void CreateNew_Should_Require_Valid_Visibility()
        {
            var msg = CommentMessage.Create("x");
            Action act = () => TicketComment.CreateNew(1, 1, "Invalid", msg, DateTime.Now);
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void ReplaceMessage_Should_Prefix_And_Update_CreatedAt()
        {
            var now = new DateTime(2026, 1, 1, 10, 0, 0);
            var c = TicketComment.CreateNew(1, 10, CommentVisibility.Public, CommentMessage.Create("old"), now);

            var later = now.AddMinutes(5);
            c.ReplaceMessage(CommentMessage.Create("new"), later);

            c.Message.Should().Be("editado: new");
            c.CreatedAt.Should().Be(later);
        }

        [Fact]
        public void ReplaceMessage_Should_Block_When_No_Real_Change()
        {
            var now = new DateTime(2026, 1, 1, 10, 0, 0);
            var c = TicketComment.CreateNew(1, 10, CommentVisibility.Public, CommentMessage.Create("same"), now);

            Action act = () => c.ReplaceMessage(CommentMessage.Create("same"), now.AddMinutes(1));
            act.Should().Throw<DomainException>();
        }
    }
}
