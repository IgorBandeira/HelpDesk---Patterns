using FluentAssertions;
using HelpDesk.Application.IdentityAccess.DTOs;
using HelpDesk.Application.IdentityAccess.UseCases.CreateUser;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.UnitTests.IdentityAccess.Fakes;

namespace HelpDesk.UnitTests.IdentityAccess.Application
{
    public class CreateUserHandler_Tests
    {
        [Fact]
        public async Task Handle_Should_Require_Manager()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Req User", "req@x.com", "Requester");

            var repo = new InMemoryUserRepository();
            var handler = new CreateUserHandler(userRead, repo);

            var cmd = new CreateUserCommand(
                AuthUserId: 10,
                Dto: new CreateUserDto
                {
                    Name = "X",
                    Email = "x@x.com",
                    Role = "Requester"
                });

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Forbidden);
        }

        [Fact]
        public async Task Handle_Should_Return_400_When_Domain_Validation_Fails()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            var handler = new CreateUserHandler(userRead, repo);

            var cmd = new CreateUserCommand(
                AuthUserId: 10,
                Dto: new CreateUserDto
                {
                    Name = "X",
                    Email = "bad",
                    Role = "Requester"
                });

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);
        }

        [Fact]
        public async Task Handle_Should_Block_When_Email_Already_Exists_CaseInsensitive()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            repo.Seed("Existing", "test@x.com", "Requester");

            var handler = new CreateUserHandler(userRead, repo);

            var cmd = new CreateUserCommand(
                AuthUserId: 10,
                Dto: new CreateUserDto
                {
                    Name = "New",
                    Email = "TEST@x.com",
                    Role = "Requester"
                });

            // Act
            var act = async () => await handler.HandleAsync(cmd);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Conflict);
        }

        [Fact]
        public async Task Handle_Should_Create_User_When_Valid()
        {
            // Arrange
            var userRead = new FakeUserReadPort();
            userRead.Seed(10, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryUserRepository();
            var handler = new CreateUserHandler(userRead, repo);

            var cmd = new CreateUserCommand(
                AuthUserId: 10,
                Dto: new CreateUserDto
                {
                    Name = "  John  ",
                    Email = "john@x.com",
                    Role = "Requester"
                });

            // Act
            var created = await handler.HandleAsync(cmd);

            // Assert
            created.Id.Should().BeGreaterThan(0);
            created.Name.Should().Be("John");
            created.Email.Should().Be("john@x.com");
            created.Role.Should().Be("Requester");
        }
    }
}