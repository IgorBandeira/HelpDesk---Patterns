using HelpDesk.Infrastructure.Operations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations.Operations
{
    public sealed class TicketActionEntityConfiguration : IEntityTypeConfiguration<TicketActionEntity>
    {
        public void Configure(EntityTypeBuilder<TicketActionEntity> builder)
        {
            builder.ToTable("TicketActions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Description).HasMaxLength(600).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasOne(x => x.Ticket)
                .WithMany(t => t.Actions)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
