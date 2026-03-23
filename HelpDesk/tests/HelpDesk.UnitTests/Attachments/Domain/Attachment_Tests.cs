using FluentAssertions;
using HelpDesk.Domain.Attachments.Aggregates;
using HelpDesk.Domain.Attachments.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.UnitTests.Attachments.Domain
{
    public class Attachment_Tests
    {
        [Fact]
        public void CreateNew_Should_Set_Fields()
        {
            var now = new DateTime(2026, 1, 1, 10, 0, 0);

            var file = AttachmentFile.Create("a.txt", "text/plain", 100);
            var key = StorageKey.Create("tickets/10/a.txt");

            var att = Attachment.CreateNew(ticketId: 10, file, key, publicUrl: "http://x", uploadedById: 5, now);

            att.TicketId.Should().Be(10);
            att.FileName.Should().Be("a.txt");
            att.ContentType.Should().Be("text/plain");
            att.SizeBytes.Should().Be(100);
            att.StorageKey.Should().Be("tickets/10/a.txt");
            att.PublicUrl.Should().Be("http://x");
            att.UploadedById.Should().Be(5);
            att.UploadedAt.Should().Be(now);
        }

        [Fact]
        public void CreateNew_Should_Throw_When_UploadedById_Invalid()
        {
            var file = AttachmentFile.Create("a.txt", "text/plain", 100);
            var key = StorageKey.Create("tickets/10/a.txt");

            Action act = () => Attachment.CreateNew(10, file, key, null, uploadedById: 0, now: DateTime.Now);

            act.Should().Throw<DomainException>();
        }
    }
}
