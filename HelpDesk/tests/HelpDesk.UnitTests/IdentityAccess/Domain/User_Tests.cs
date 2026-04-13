using FluentAssertions;
using HelpDesk.Domain.IdentityAccess.Aggregates;
using HelpDesk.Domain.IdentityAccess.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.UnitTests.IdentityAccess.Domain
{
    public class User_Tests
    {
        [Fact]
        public void CreateNew_Should_Set_Name_Email_And_Role()
        {
            // Arrange
            var name = UserName.Create("  John  ");
            var email = EmailAddress.Create("john@x.com");
            var role = UserRole.Create("Requester");

            // Act
            var user = User.CreateNew(name, email, role);

            // Assert
            user.Name.Value.Should().Be("John");
            user.Email.Value.Should().Be("john@x.com");
            user.Role.Value.Should().Be("Requester");

            user.Id.Should().Be(0);
        }

        [Fact]
        public void UpdateName_Should_Replace_Name()
        {
            // Arrange
            var user = User.CreateNew(
                UserName.Create("John"),
                EmailAddress.Create("john@x.com"),
                UserRole.Create("Requester"));

            // Act
            user.ReplaceName(UserName.Create("  Johnny  "));

            // Assert
            user.Name.Value.Should().Be("Johnny");
        }

        [Fact]
        public void UpdateEmail_Should_Replace_Email()
        {
            // Arrange
            var user = User.CreateNew(
                UserName.Create("John"),
                EmailAddress.Create("john@x.com"),
                UserRole.Create("Requester"));

            // Act
            user.ReplaceEmail(EmailAddress.Create("johnny@x.com"));

            // Assert
            user.Email.Value.Should().Be("johnny@x.com");
        }

        [Fact]
        public void UpdateRole_Should_Replace_Role()
        {
            // Arrange
            var user = User.CreateNew(
                UserName.Create("John"),
                EmailAddress.Create("john@x.com"),
                UserRole.Create("Requester"));

            // Act
            user.ReplaceRole(UserRole.Create("Agent"));

            // Assert
            user.Role.Value.Should().Be("Agent");
        }

        [Fact]
        public void CreateNew_Should_Throw_When_Given_Invalid_ValueObjects()
        {

            Action act = () =>
            {
                var user = User.CreateNew(
                    UserName.Create("John"),
                    EmailAddress.Create("bad"),
                    UserRole.Create("Requester"));
            };

            act.Should().Throw<DomainException>();
        }
    }
}