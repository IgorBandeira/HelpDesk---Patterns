using HelpDesk.Domain.ServiceCatalog.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;
using FluentAssertions;

namespace HelpDesk.UnitTests.ServiceCatalog.Domain
{
    public class CategoryName_Tests
    {
        [Fact]
        public void Create_Should_Throw_When_Null_Or_Whitespace()
        {
            Action a1 = () => CategoryName.Create(null);
            a1.Should().Throw<DomainException>();

            Action a2 = () => CategoryName.Create("   ");
            a2.Should().Throw<DomainException>();
        }

        [Fact]
        public void Create_Should_Trim()
        {
            var n = CategoryName.Create("  Network  ");
            n.Value.Should().Be("Network");
        }

        [Fact]
        public void Create_Should_Enforce_MaxLength()
        {
            var tooLong = new string('a', CategoryName.MaxLength + 1);

            Action act = () => CategoryName.Create(tooLong);

            act.Should().Throw<DomainException>();
        }
    }
}
