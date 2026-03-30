using HelpDesk.Infrastructure.Attachments.Models;
using HelpDesk.Infrastructure.Collaboration.Models;
using HelpDesk.Infrastructure.IdentityAccess.Models;
using HelpDesk.Infrastructure.ServiceCatalog.Models;
using HelpDesk.Infrastructure.Operations.Models;
using HelpDesk.Infrastructure.Ticketing.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users => Set<UserEntity>();
        public DbSet<TicketEntity> Tickets => Set<TicketEntity>();
        public DbSet<TicketActionEntity> TicketActions => Set<TicketActionEntity>();
        public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
        public DbSet<CommentEntity> Comments => Set<CommentEntity>();
        public DbSet<AttachmentEntity> Attachments => Set<AttachmentEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
