using FluentAssertions;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.Ticketing.Aggregates;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Domain.Ticketing.ValueObjects;

namespace HelpDesk.UnitTests.Ticketing.Domain
{
    public class Ticket_Tests
    {
        [Fact]
        public void CreateNew_Should_Set_Defaults_And_Sla()
        {
            var now = new DateTime(2026, 1, 1, 10, 0, 0);
            var t = Ticket.CreateNew(
                TicketTitle.Create("T"),
                TicketDescription.Create("D"),
                TicketPriority.Media,
                requesterId: 10,
                categoryId: 20,
                now: now);

            t.Status.Should().Be(TicketStatus.Novo);
            t.CreatedAt.Should().Be(now);
            t.SlaStartAt.Should().Be(now);
            t.SlaDueAt.Should().Be(now + TimeSpan.FromHours(48));
            t.RequesterId.Should().Be(10);
            t.CategoryId.Should().Be(20);
        }

        [Fact]
        public void Update_Should_Block_When_Closed_Or_Canceled()
        {
            var now = new DateTime(2026, 1, 1, 10, 0, 0);

            var canceled = Ticket.CreateNew(
                TicketTitle.Create("T"),
                TicketDescription.Create("D"),
                TicketPriority.Media,
                requesterId: 10,
                categoryId: 20,
                now: now);

            canceled.Cancel(10, "Reason", now.AddMinutes(1));

            Action actCanceled = () =>
            {
                _ = canceled.Update(
                    newTitle: TicketTitle.Create("New Title"),
                    newDescription: null,
                    newPriority: null,
                    newCategoryId: null,
                    now: now.AddMinutes(2));
            };

            actCanceled.Should().Throw<DomainException>()
                .WithMessage("*inativo*");

            var closed = Ticket.CreateNew(
                TicketTitle.Create("T"),
                TicketDescription.Create("D"),
                TicketPriority.Media,
                requesterId: 10,
                categoryId: 20,
                now: now);

            closed.AssignToAgent(agentId: 99, now: now.AddMinutes(1));              
            closed.ChangeStatus(TicketStatus.EmAndamento, actorUserId: 99, now: now.AddMinutes(2)); 
            closed.ChangeStatus(TicketStatus.Resolvido, actorUserId: 99, now: now.AddMinutes(3));   
            closed.ChangeStatus(TicketStatus.Fechado, actorUserId: 10, now: now.AddMinutes(4));    

            Action actClosed = () =>
            {
                _ = closed.Update(
                    newTitle: TicketTitle.Create("New Title"),
                    newDescription: null,
                    newPriority: null,
                    newCategoryId: null,
                    now: now.AddMinutes(5));
            };

            actClosed.Should().Throw<DomainException>()
                .WithMessage("*inativo*");
        }

        [Fact]
        public void Update_Should_Recalculate_Sla_When_Priority_Changes()
        {
            var now = new DateTime(2026, 1, 1, 10, 0, 0);
            var t = Ticket.CreateNew(TicketTitle.Create("T"), TicketDescription.Create("D"),
                TicketPriority.Media, 10, 20, now);

            var changed = t.Update(
                newTitle: null,
                newDescription: null,
                newPriority: TicketPriority.Critica,
                newCategoryId: null,
                now: now.AddHours(2));

            changed.Should().BeTrue();
            t.Priority.Should().Be(TicketPriority.Critica);
            t.SlaStartAt.Should().Be(now.AddHours(2));
            t.SlaDueAt.Should().Be(now.AddHours(2) + TimeSpan.FromHours(8));
        }

        [Fact]
        public void AssignToAgent_Should_Set_Assignee_And_Move_From_Novo_To_EmAnalise()
        {
            var now = new DateTime(2026, 1, 1, 10, 0, 0);
            var t = Ticket.CreateNew(TicketTitle.Create("T"), TicketDescription.Create("D"),
                TicketPriority.Media, 10, 20, now);

            t.AssignToAgent(agentId: 99, now: now.AddMinutes(5));

            t.AssigneeId.Should().Be(99);
            t.AssignedAt.Should().Be(now.AddMinutes(5));
            t.Status.Should().Be(TicketStatus.EmAnalise);
        }

        [Fact]
        public void ChangeStatus_Should_Enforce_Allowed_Transitions_And_Actor_Rules()
        {
            var now = new DateTime(2026, 1, 1, 10, 0, 0);
            var t = Ticket.CreateNew(TicketTitle.Create("T"), TicketDescription.Create("D"),
                TicketPriority.Media, requesterId: 10, categoryId: 20, now);

            t.AssignToAgent(agentId: 99, now: now.AddMinutes(1));

            Action notAssignee = () => t.ChangeStatus(TicketStatus.EmAndamento, actorUserId: 10, now: now.AddMinutes(2));
            notAssignee.Should().Throw<DomainException>();

            t.ChangeStatus(TicketStatus.EmAndamento, actorUserId: 99, now: now.AddMinutes(2));
            t.Status.Should().Be(TicketStatus.EmAndamento);

            t.ChangeStatus(TicketStatus.Resolvido, actorUserId: 99, now: now.AddMinutes(3));
            t.Status.Should().Be(TicketStatus.Resolvido);

            Action notRequester = () => t.ChangeStatus(TicketStatus.Fechado, actorUserId: 99, now: now.AddMinutes(4));
            notRequester.Should().Throw<DomainException>();

            t.ChangeStatus(TicketStatus.Fechado, actorUserId: 10, now: now.AddMinutes(4));
            t.Status.Should().Be(TicketStatus.Fechado);
            t.ClosedAt.Should().Be(now.AddMinutes(4));
        }

        [Fact]
        public void Reopen_Should_Require_Resolved_Or_Closed_And_Reason()
        {
            var now = new DateTime(2026, 1, 1, 10, 0, 0);
            var t = Ticket.CreateNew(TicketTitle.Create("T"), TicketDescription.Create("D"),
                TicketPriority.Media, requesterId: 10, categoryId: 20, now);

            Action invalid = () => t.Reopen("x", now.AddHours(1));
            invalid.Should().Throw<DomainException>();

            t.AssignToAgent(99, now.AddMinutes(1));
            t.ChangeStatus(TicketStatus.EmAndamento, 99, now.AddMinutes(2));
            t.ChangeStatus(TicketStatus.Resolvido, 99, now.AddMinutes(3));
            t.ChangeStatus(TicketStatus.Fechado, 10, now.AddMinutes(4));

            Action noReason = () => t.Reopen("   ", now.AddMinutes(5));
            noReason.Should().Throw<DomainException>();

            t.Reopen("Need more work", now.AddMinutes(5));
            t.Status.Should().Be(TicketStatus.EmAnalise);
            t.ClosedAt.Should().BeNull();
            t.SlaStartAt.Should().Be(now.AddMinutes(5));
        }

        [Fact]
        public void Cancel_Should_Allow_Only_Novo_Or_EmAnalise_And_Require_Reason()
        {
            var now = new DateTime(2026, 1, 1, 10, 0, 0);
            var t = Ticket.CreateNew(TicketTitle.Create("T"), TicketDescription.Create("D"),
                TicketPriority.Media, 10, 20, now);

            Action noReason = () => t.Cancel(10, "   ", now.AddMinutes(1));
            noReason.Should().Throw<DomainException>();

            t.Cancel(10, "Mistake", now.AddMinutes(1));
            t.Status.Should().Be(TicketStatus.Cancelado);
            t.ClosedAt.Should().Be(now.AddMinutes(1));
        }
    }
}
