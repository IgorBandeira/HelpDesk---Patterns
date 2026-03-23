using HelpDesk.Domain.ServiceCatalog.Aggregates;
using HelpDesk.Domain.ServiceCatalog.ValueObjects;
using FluentAssertions;

namespace HelpDesk.UnitTests.ServiceCatalog.Domain
{
    public class Category_Tests
    {
        [Fact]
        public void CreateNew_Should_Set_Name_And_ParentId()
        {
            var name = CategoryName.Create("Hardware");

            var root = Category.CreateNew(name, parentId: null);
            root.Name.Value.Should().Be("Hardware");
            root.ParentId.Should().BeNull();

            var child = Category.CreateNew(CategoryName.Create("Mouse"), parentId: 123);
            child.Name.Value.Should().Be("Mouse");
            child.ParentId.Should().Be(123);
        }
    }
}
