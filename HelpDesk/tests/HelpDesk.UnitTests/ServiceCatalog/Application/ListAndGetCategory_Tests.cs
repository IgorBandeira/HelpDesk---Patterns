using FluentAssertions;
using HelpDesk.Application.ServiceCatalog.UseCases.GetCategoryById;
using HelpDesk.Application.ServiceCatalog.UseCases.ListCategories;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.UnitTests.ServiceCatalog.Fakes;

namespace HelpDesk.UnitTests.ServiceCatalog.Application
{
    public class ListAndGetCategory_Tests
    {
        [Fact]
        public async Task List_Should_Filter_Page_Order_And_Format_Name_With_Parent()
        {
            var repo = new InMemoryCategoryRepository();

            var parent = repo.Seed("Parent");
            var child1 = repo.Seed("ChildA", parentId: parent.Id);
            var child2 = repo.Seed("ChildB", parentId: parent.Id);
            _ = repo.Seed("Other");

            var handler = new ListCategoriesHandler(repo);

            var items = await handler.HandleAsync(new ListCategoriesQuery(
                NameContains: null,
                ParentId: parent.Id,
                Page: 1,
                PageSize: 20));

            items.Should().HaveCount(2);
            items.Select(i => i.Name).Should().Contain(new[]
            {
            "Parent - ChildA",
            "Parent - ChildB"
        });

            var items2 = await handler.HandleAsync(new ListCategoriesQuery(
                NameContains: "Child",
                ParentId: null,
                Page: 1,
                PageSize: 20));

            items2.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_Should_Return_404_When_NotFound_And_Format_Name()
        {
            var repo = new InMemoryCategoryRepository();
            var parent = repo.Seed("Parent");
            var child = repo.Seed("Child", parentId: parent.Id);

            var handler = new GetCategoryByIdHandler(repo);

            var found = await handler.HandleAsync(new GetCategoryByIdQuery(child.Id));
            found.Name.Should().Be("Parent - Child");

            var act = async () => await handler.HandleAsync(new GetCategoryByIdQuery(999));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.NotFound);
        }
    }
}
