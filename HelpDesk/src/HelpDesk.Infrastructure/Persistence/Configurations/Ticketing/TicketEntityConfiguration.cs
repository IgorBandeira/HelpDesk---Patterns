using HelpDesk.Infrastructure.Ticketing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations.Ticketing
{
    public sealed class TicketEntityConfiguration : IEntityTypeConfiguration<TicketEntity>
    {
        public void Configure(EntityTypeBuilder<TicketEntity> builder)
        {
            builder.ToTable("Tickets");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).HasMaxLength(180).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
            builder.Property(x => x.PriorityLevel).HasMaxLength(20).IsRequired();

            builder.HasOne(x => x.Requester)
             .WithMany(x => x.RequestedTickets)
             .HasForeignKey(x => x.RequesterId)
             .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Assignee)
             .WithMany(x => x.AssignedTickets)
             .HasForeignKey(x => x.AssigneeId)
             .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Category)
             .WithMany()
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.SetNull);
        }
    }
}