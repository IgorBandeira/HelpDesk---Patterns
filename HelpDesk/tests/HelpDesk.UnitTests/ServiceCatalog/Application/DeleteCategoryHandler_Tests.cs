using FluentAssertions;
using HelpDesk.Application.ServiceCatalog.UseCases.DeleteCategory;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.UnitTests.IdentityAccess.Fakes;
using HelpDesk.UnitTests.ServiceCatalog.Fakes;

namespace HelpDesk.UnitTests.ServiceCatalog.Application
{
    public class DeleteCategoryHandler_Tests
    {
        [Fact]
        public async Task Delete_Should_Block_When_Has_Children_Or_Active_Tickets()
        {
            var userRead = new FakeUserReadPort();
            userRead.Seed(1, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryCategoryRepository();
            var tickets = new FakeTicketQueryPort();

            var parent = repo.Seed("Parent");
            _ = repo.Seed("Child", parentId: parent.Id);

            var handler = new DeleteCategoryHandler(userRead, repo, tickets);

            var cmd = new DeleteCategoryCommand(UserId: 1, CategoryId: parent.Id);

            var act1 = async () => await handler.HandleAsync(cmd);
            (await act1.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Conflict)
                .Where(e => e.Message.Contains("subcategorias", StringComparison.OrdinalIgnoreCase));

            var userRead2 = new FakeUserReadPort();
            userRead2.Seed(1, "Manager User", "manager@x.com", "Manager");

            var repo2 = new InMemoryCategoryRepository();
            var tickets2 = new FakeTicketQueryPort();

            var cat = repo2.Seed("ToDelete");
            tickets2.MarkActiveTicketsForCategory(cat.Id);

            var handler2 = new DeleteCategoryHandler(userRead2, repo2, tickets2);
            var cmd2 = new DeleteCategoryCommand(1, cat.Id);

            var act2 = async () => await handler2.HandleAsync(cmd2);
            (await act2.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Conflict)
                .Where(e => e.Message.Contains("chamados ativos", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Delete_Should_Require_Manager()
        {
            var userRead = new FakeUserReadPort();
            userRead.Seed(1, "Requester User", "requester@x.com", "Requester");

            var repo = new InMemoryCategoryRepository();
            var tickets = new FakeTicketQueryPort();

            var cat = repo.Seed("X");
            var handler = new DeleteCategoryHandler(userRead, repo, tickets);

            var act = async () => await handler.HandleAsync(new DeleteCategoryCommand(1, cat.Id));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Forbidden);
        }
    }
}