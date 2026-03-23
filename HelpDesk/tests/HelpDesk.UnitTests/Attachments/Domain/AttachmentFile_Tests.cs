using FluentAssertions;
using HelpDesk.Domain.Attachments.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.UnitTests.Attachments.Domain
{
    public class AttachmentFile_Tests
    {
        [Fact]
        public void Create_Should_Validate_Size_And_Blocked_Extensions_And_Default_ContentType()
        {
            Action a1 = () => AttachmentFile.Create("a.txt", "text/plain", 0);
            a1.Should().Throw<DomainException>();

            Action a2 = () => AttachmentFile.Create("a.txt", "text/plain", 10 * 1024 * 1024 + 1);
            a2.Should().Throw<DomainException>()
              .WithMessage("*10MB*");

            Action a3 = () => AttachmentFile.Create("virus.exe", "application/octet-stream", 100);
            a3.Should().Throw<DomainException>();

            var ok = AttachmentFile.Create("a.txt", null, 100);
            ok.ContentType.Should().Be("application/octet-stream");
        }
    }
}
