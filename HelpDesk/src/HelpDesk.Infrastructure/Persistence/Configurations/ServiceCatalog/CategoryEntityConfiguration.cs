using HelpDesk.Infrastructure.ServiceCatalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations.ServiceCatalog
{
    public sealed class CategoryEntityConfiguration : IEntityTypeConfiguration<CategoryEntity>
    {
        public void Configure(EntityTypeBuilder<CategoryEntity> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(180);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.HasData(
               new CategoryEntity { Id = 1, Name = "Infraestrutura", ParentId = null },
               new CategoryEntity { Id = 2, Name = "Aplicações", ParentId = null },
               new CategoryEntity { Id = 3, Name = "Redes", ParentId = null },
               new CategoryEntity { Id = 4, Name = "Segurança", ParentId = null },
               new CategoryEntity { Id = 5, Name = "Suporte", ParentId = null },

               new CategoryEntity { Id = 6, Name = "Serviços em Nuvem", ParentId = 1 },
               new CategoryEntity { Id = 7, Name = "Bancos de Dados", ParentId = 2 },
               new CategoryEntity { Id = 8, Name = "Firewall", ParentId = 4 },
               new CategoryEntity { Id = 9, Name = "Central de Ajuda", ParentId = 5 },
               new CategoryEntity { Id = 10, Name = "LAN/WAN", ParentId = 3 }
            );
        }
    }

}
