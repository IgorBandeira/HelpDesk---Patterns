using HelpDesk.Infrastructure.IdentityAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations.IdentityAccess
{
    public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Email).IsRequired();
            builder.Property(x => x.Role).IsRequired();

            builder.HasIndex(x => x.Email).IsUnique();

            builder.HasData(
                new UserEntity { Id = 1, Name = "Alice Johnson", Email = "alice.johnson@acme.com", Role = "Requester" },
                new UserEntity { Id = 2, Name = "Bob Miller", Email = "bob.miller@acme.com", Role = "Agent" },
                new UserEntity { Id = 3, Name = "Clara Thompson", Email = "clara.thompson@acme.com", Role = "Manager" },
                new UserEntity { Id = 4, Name = "David Anderson", Email = "david.anderson@acme.com", Role = "Requester" },
                new UserEntity { Id = 5, Name = "Emily Carter", Email = "emily.carter@acme.com", Role = "Agent" },
                new UserEntity { Id = 6, Name = "Frank Harris", Email = "frank.harris@acme.com", Role = "Manager" },
                new UserEntity { Id = 7, Name = "Grace Lewis", Email = "grace.lewis@acme.com", Role = "Requester" },
                new UserEntity { Id = 8, Name = "Henry Clark", Email = "henry.clark@acme.com", Role = "Agent" },
                new UserEntity { Id = 9, Name = "Isabella Scott", Email = "isabella.scott@acme.com", Role = "Manager" },
                new UserEntity { Id = 10, Name = "Jack Wilson", Email = "jack.wilson@acme.com", Role = "Requester" }
            );
        }
    }
}
