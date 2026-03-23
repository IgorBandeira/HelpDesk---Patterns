using FluentAssertions;
using HelpDesk.Application.IdentityAccess.UseCases.DeleteUser;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.UnitTests.IdentityAccess.Fakes;

namespace HelpDesk.UnitTests.IdentityAccess.Application
{
    public class DeleteUserHandler_Tests
    {
        [Fact]
        public async Task Handle_Should_Require_Manager()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Requester User", "requester@x.com", "Requester");

            var repo = new InMemoryUserRepository();
            var tickets = new FakeTicketUserQueryPort();

            var userToDelete = repo.Seed("R", "r@x.com", "Requester");

            var handler = new DeleteUserHandler(userRead, repo, tickets);

            var cmd = new DeleteUserCommand(
                Id: userToDelete.Id,
                AuthUserId: 10);

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Forbidden);
        }

        [Fact]
        public async Task Handle_Should_Return_404_When_User_Not_Found()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            var tickets = new FakeTicketUserQueryPort();

            var handler = new DeleteUserHandler(userRead, repo, tickets);

            var cmd = new DeleteUserCommand(
                Id: 999,
                AuthUserId: 10);

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.NotFound);
        }

        [Fact]
        public async Task Handle_Should_Block_When_User_Has_Active_Tickets_As_Assignee()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            var tickets = new FakeTicketUserQueryPort();

            var userToDelete = repo.Seed("A", "a@x.com", "Agent");
            tickets.MarkActiveAsAssignee(userToDelete.Id);

            var handler = new DeleteUserHandler(userRead, repo, tickets);

            var cmd = new DeleteUserCommand(
                Id: userToDelete.Id,
                AuthUserId: 10);

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Conflict);
        }

        [Fact]
        public async Task Handle_Should_Block_When_User_Has_Active_Tickets_As_Requester()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            var tickets = new FakeTicketUserQueryPort();

            var userToDelete = repo.Seed("R", "r@x.com", "Requester");
            tickets.MarkActiveAsRequester(userToDelete.Id);

            var handler = new DeleteUserHandler(userRead, repo, tickets);

            var cmd = new DeleteUserCommand(
                Id: userToDelete.Id,
                AuthUserId: 10);

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Conflict);
        }

        [Fact]
        public async Task Handle_Should_Delete_When_No_Active_Tickets()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            var tickets = new FakeTicketUserQueryPort();

            var userToDelete = repo.Seed("R", "r@x.com", "Requester");

            var handler = new DeleteUserHandler(userRead, repo, tickets);

            var cmd = new DeleteUserCommand(
                Id: userToDelete.Id,
                AuthUserId: 10);

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            await act.Should().NotThrowAsync();
            (await repo.GetByIdAsync(userToDelete.Id)).Should().BeNull();
        }
    }
}