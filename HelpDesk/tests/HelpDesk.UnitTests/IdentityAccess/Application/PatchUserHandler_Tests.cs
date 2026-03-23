using FluentAssertions;
using HelpDesk.Application.IdentityAccess.DTOs;
using HelpDesk.Application.IdentityAccess.UseCases.PatchUser;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.UnitTests.IdentityAccess.Fakes;

namespace HelpDesk.UnitTests.IdentityAccess.Application
{
    public class PatchUserHandler_Tests
    {
        [Fact]
        public async Task Handle_Should_Require_Manager()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Requester User", "requester@x.com", "Requester");

            var repo = new InMemoryUserRepository();
            var user = repo.Seed("R", "r@x.com", "Requester");

            var handler = new PatchUserHandler(userRead, repo);

            var cmd = new PatchUserCommand(
                Id: user.Id,
                AuthUserId: 10,
                Dto: new UpdateUserDto { Name = "New" });

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
            var handler = new PatchUserHandler(userRead, repo);

            var cmd = new PatchUserCommand(
                Id: 999,
                AuthUserId: 10,
                Dto: new UpdateUserDto { Name = "New" });

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.NotFound);
        }

        [Fact]
        public async Task Handle_Should_Return_400_When_No_Changes_Detected()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            var user = repo.Seed("John", "john@x.com", "Requester");

            var handler = new PatchUserHandler(userRead, repo);

            var cmd = new PatchUserCommand(
                Id: user.Id,
                AuthUserId: 10,
                Dto: new UpdateUserDto());

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);
        }

        [Fact]
        public async Task Handle_Should_Return_400_When_Email_Format_Is_Invalid()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            var user = repo.Seed("John", "john@x.com", "Requester");

            var handler = new PatchUserHandler(userRead, repo);

            var cmd = new PatchUserCommand(
                Id: user.Id,
                AuthUserId: 10,
                Dto: new UpdateUserDto { Email = "bad" });

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);
        }

        [Fact]
        public async Task Handle_Should_Return_409_When_Email_Already_Exists()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();

            var user1 = repo.Seed("A", "a@x.com", "Requester");
            var user2 = repo.Seed("B", "b@x.com", "Requester");

            var handler = new PatchUserHandler(userRead, repo);

            var cmd = new PatchUserCommand(
                Id: user2.Id,
                AuthUserId: 10,
                Dto: new UpdateUserDto { Email = "A@x.com" });

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Conflict);
        }

        [Fact]
        public async Task Handle_Should_Return_400_When_Role_Is_Invalid()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            var user = repo.Seed("John", "john@x.com", "Requester");

            var handler = new PatchUserHandler(userRead, repo);

            var cmd = new PatchUserCommand(
                Id: user.Id,
                AuthUserId: 10,
                Dto: new UpdateUserDto { Role = "Admin" });

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);
        }

        [Fact]
        public async Task Handle_Should_Update_Name_Email_And_Role_When_Valid()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            var user = repo.Seed("John", "john@x.com", "Requester");

            var handler = new PatchUserHandler(userRead, repo);

            var cmd = new PatchUserCommand(
                Id: user.Id,
                AuthUserId: 10,
                Dto: new UpdateUserDto
                {
                    Name = "  Johnny  ",
                    Email = "johnny@x.com",
                    Role = "Agent"
                });

            // Act
            var updated = await handler.HandleAsync(cmd);

            // Assert
            updated.Id.Should().Be(user.Id);
            updated.Name.Should().Be("Johnny");
            updated.Email.Should().Be("johnny@x.com");
            updated.Role.Should().Be("Agent");
        }
    }
}