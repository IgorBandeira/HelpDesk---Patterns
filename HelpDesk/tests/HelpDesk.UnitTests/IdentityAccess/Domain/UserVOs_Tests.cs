using FluentAssertions;
using HelpDesk.Domain.IdentityAccess.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.UnitTests.IdentityAccess.Domain
{
    public class UserVOs_Tests
    {
        [Fact]
        public void EmailAddress_Create_Should_Require_Valid_Format()
        {
            Action a1 = () => EmailAddress.Create(null);
            a1.Should().Throw<DomainException>();

            Action a2 = () => EmailAddress.Create("   ");
            a2.Should().Throw<DomainException>();

            Action a3 = () => EmailAddress.Create("bad");
            a3.Should().Throw<DomainException>()
              .WithMessage("*e-mail*");

            var ok = EmailAddress.Create("x@x.com");
            ok.Value.Should().Be("x@x.com");
        }

        [Fact]
        public void UserRole_Create_Should_Allow_Only_Requester_Agent_Manager()
        {
            Action a1 = () => UserRole.Create(null);
            a1.Should().Throw<DomainException>();

            Action a2 = () => UserRole.Create("Admin");
            a2.Should().Throw<DomainException>()
              .WithMessage("*papel*");

            UserRole.Create("Requester").Value.Should().Be("Requester");
            UserRole.Create("agent").Value.Should().Be("agent");
            UserRole.Create("MANAGER").Value.Should().Be("MANAGER");
        }

        [Fact]
        public void UserName_Create_Should_Require_And_Trim()
        {
            Action a1 = () => UserName.Create(null);
            a1.Should().Throw<DomainException>();

            Action a2 = () => UserName.Create("   ");
            a2.Should().Throw<DomainException>();

            var ok = UserName.Create("  John  ");
            ok.Value.Should().Be("John");
        }
    }
}
