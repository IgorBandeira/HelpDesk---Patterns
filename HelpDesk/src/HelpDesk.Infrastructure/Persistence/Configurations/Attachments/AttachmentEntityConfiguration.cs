using HelpDesk.Infrastructure.Attachments.Models;
using HelpDesk.Infrastructure.Ticketing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations.Attachments
{
    public sealed class AttachmentEntityConfiguration : IEntityTypeConfiguration<AttachmentEntity>
    {
        public void Configure(EntityTypeBuilder<AttachmentEntity> builder)
        {
            builder.ToTable("Attachments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.ContentType).HasMaxLength(128).IsRequired();

            builder.Property(x => x.StorageKey).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.PublicUrl).HasMaxLength(2000);

            builder.Property(x => x.UploadedAt).IsRequired();

            builder.HasOne<TicketEntity>()
             .WithMany(t => t.Attachments)
             .HasForeignKey(x => x.TicketId);

            builder.HasOne(x => x.UploadedBy)
             .WithMany()
             .HasForeignKey(x => x.UploadedById)
             .OnDelete(DeleteBehavior.SetNull);
        }
    }
}