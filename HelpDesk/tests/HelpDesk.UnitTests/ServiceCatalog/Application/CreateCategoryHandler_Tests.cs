using FluentAssertions;
using HelpDesk.Application.ServiceCatalog.DTOs;
using HelpDesk.Application.ServiceCatalog.UseCases.CreateCategory;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.UnitTests.IdentityAccess.Fakes;
using HelpDesk.UnitTests.ServiceCatalog.Fakes;

namespace HelpDesk.UnitTests.ServiceCatalog.Application
{
    public class CreateCategoryHandler_Tests
    {
        [Fact]
        public async Task Create_Should_Require_Manager()
        {
            var userRead = new FakeUserReadPort();
            userRead.Seed(1, "Requester User", "requester@x.com", "Requester");

            var repo = new InMemoryCategoryRepository();
            var handler = new CreateCategoryHandler(userRead, repo);

            var cmd = new CreateCategoryCommand(
                UserId: 1,
                Dto: new CreateCategoryDto { Name = "Hardware", ParentId = null }
            );

            var act = async () => await handler.HandleAsync(cmd);

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Forbidden);
        }

        [Fact]
        public async Task Create_Should_Validate_Parent_Exists_And_Depth()
        {
            var userRead = new FakeUserReadPort();
            userRead.Seed(1, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryCategoryRepository();
            var handler = new CreateCategoryHandler(userRead, repo);

            var cmdBadParent = new CreateCategoryCommand(
                1,
                new CreateCategoryDto { Name = "Child", ParentId = 999 }
            );

            var act1 = async () => await handler.HandleAsync(cmdBadParent);
            (await act1.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);

            var root = repo.Seed("Root");
            var parentIsSub = repo.Seed("SubParent", parentId: root.Id);

            var cmdDepthInvalid = new CreateCategoryCommand(
                1,
                new CreateCategoryDto { Name = "GrandChild", ParentId = parentIsSub.Id }
            );

            var act2 = async () => await handler.HandleAsync(cmdDepthInvalid);
            (await act2.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Conflict);
        }

        [Fact]
        public async Task Create_Should_Enforce_Unique_Name_CaseSensitive()
        {
            var userRead = new FakeUserReadPort();
            userRead.Seed(1, "Manager User", "manager@x.com", "Manager");

            var repo = new InMemoryCategoryRepository();
            repo.Seed("Network");

            var handler = new CreateCategoryHandler(userRead, repo);

            var cmd = new CreateCategoryCommand(
                1,
                new CreateCategoryDto { Name = "Network", ParentId = null }
            );

            var act = async () => await handler.HandleAsync(cmd);

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Conflict);

            var cmd2 = new CreateCategoryCommand(
                1,
                new CreateCategoryDto { Name = "network", ParentId = null }
            );

            var created = await handler.HandleAsync(cmd2);
            created.Name.Should().Be("network");
        }
    }
}