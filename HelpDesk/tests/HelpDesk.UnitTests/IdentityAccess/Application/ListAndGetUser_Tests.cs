using FluentAssertions;
using HelpDesk.Application.IdentityAccess.UseCases.GetUserById;
using HelpDesk.Application.IdentityAccess.UseCases.ListUsers;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.UnitTests.IdentityAccess.Fakes;

namespace HelpDesk.UnitTests.IdentityAccess.Application
{
    public class ListAndGetUser_Tests
    {
        [Fact]
        public async Task Handle_Should_Return_400_When_Role_Is_Invalid()
        {
            // Arrange
            var repo = new InMemoryUserRepository();
            var handler = new ListUsersHandler(repo);

            var query = new ListUsersQuery(
                Role: "Admin",
                Email: null,
                Name: null,
                Page: 1,
                PageSize: 20);

            // Act
            var act = async () => await handler.HandleAsync(query);

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);
        }

        [Fact]
        public async Task Handle_Should_Filter_By_Role_Email_Name_And_Page()
        {
            // Arrange
            var repo = new InMemoryUserRepository();

            repo.Seed("John", "john@x.com", "Requester");
            repo.Seed("Johnny", "johnny@x.com", "Agent");
            repo.Seed("Mary", "mary@x.com", "Manager");
            repo.Seed("Joana", "joana@x.com", "Requester");

            var handler = new ListUsersHandler(repo);

            // Act
            var byRole = await handler.HandleAsync(new ListUsersQuery(
                Role: "Requester",
                Email: null,
                Name: null,
                Page: 1,
                PageSize: 20));

            // Assert
            byRole.Should().HaveCount(2);
            byRole.All(u => u.Role.Equals("Requester", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();

            // Act
            var byEmail = await handler.HandleAsync(new ListUsersQuery(
                Role: null,
                Email: "JOHN", 
                Name: null,
                Page: 1,
                PageSize: 20));

            // Assert
            byEmail.Should().HaveCount(2);

            // Act
            var byName = await handler.HandleAsync(new ListUsersQuery(
                Role: null,
                Email: null,
                Name: "jo", 
                Page: 1,
                PageSize: 20));

            // Assert
            byName.Should().HaveCount(3);

            // Act
            var page1 = await handler.HandleAsync(new ListUsersQuery(
                Role: null,
                Email: null,
                Name: null,
                Page: 1,
                PageSize: 2));

            var page2 = await handler.HandleAsync(new ListUsersQuery(
                Role: null,
                Email: null,
                Name: null,
                Page: 2,
                PageSize: 2));

            // Assert
            page1.Should().HaveCount(2);
            page2.Should().HaveCount(2);
            page1.Select(x => x.Id).Intersect(page2.Select(x => x.Id)).Should().BeEmpty();
        }
        [Fact]
        public async Task Handle_Should_Return_404_When_User_Not_Found()
        {
            // Arrange
            var repo = new InMemoryUserRepository();
            var tickets = new FakeTicketUserQueryPort();
            var handler = new GetUserByIdHandler(repo, tickets);

            // Act
            var act = async () => await handler.HandleAsync(new GetUserByIdQuery(999));

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.NotFound);
        }

        [Fact]
        public async Task Handle_Should_Return_User_With_Requested_And_Assigned_Tickets()
        {
            // Arrange
            var repo = new InMemoryUserRepository();
            var user = repo.Seed("R", "r@x.com", "Requester");

            var tickets = new FakeTicketUserQueryPortWithData();
            tickets.SetRequested(user.Id, new[]
            {
            new UserTicketListItem(1, "T1", "New", "Low")
        });

            tickets.SetAssigned(user.Id, new[]
            {
            new UserTicketListItem(2, "T2", "InProgress", "High")
        });

            var handler = new GetUserByIdHandler(repo, tickets);

            // Act
            var dto = await handler.HandleAsync(new GetUserByIdQuery(user.Id));

            // Assert
            dto.RequestedTickets.Should().ContainSingle(t => t.Id == 1 && t.Title == "T1");
            dto.AssignedTickets.Should().ContainSingle(t => t.Id == 2 && t.Title == "T2");
        }
    }
}
