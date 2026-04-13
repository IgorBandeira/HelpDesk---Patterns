using HelpDesk.Infrastructure.Collaboration.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations.Collaboration
{
    public sealed class CommentEntityConfiguration : IEntityTypeConfiguration<CommentEntity>
    {
        public void Configure(EntityTypeBuilder<CommentEntity> builder)
        {
            builder.ToTable("TicketComments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Visibility)
                .HasMaxLength(16)
                .IsRequired();

            builder.Property(x => x.Message)
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .IsRequired();

            builder.HasOne(x => x.Ticket)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.TicketId);

            builder.HasOne(x => x.Author)
                .WithMany()
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}