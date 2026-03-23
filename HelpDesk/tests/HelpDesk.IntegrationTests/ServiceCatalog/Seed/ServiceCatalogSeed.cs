using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.ServiceCatalog.Models;
using HelpDesk.Infrastructure.Ticketing.Models;

namespace HelpDesk.IntegrationTests.ServiceCatalog.Seed
{
    public static class ServiceCatalogSeed
    {
        public static async Task<int> SeedCategoryAsync(AppDbContext db, string name, int? parentId = null)
        {
            var c = new CategoryEntity
            {
                Name = name,
                ParentId = parentId
            };

            db.Categories.Add(c);
            await db.SaveChangesAsync();
            return c.Id;
        }

        public static async Task<int> SeedRootCategoryAsync(AppDbContext db, string name = "Hardware")
        {
            var c = new CategoryEntity
            {
                Name = name,
                ParentId = null
            };

            db.Categories.Add(c);
            await db.SaveChangesAsync();
            return c.Id;
        }

        public static async Task<int> SeedSubCategoryAsync(AppDbContext db, int parentId, string name = "Notebook")
        {
            var c = new CategoryEntity
            {
                Name = name,
                ParentId = parentId
            };

            db.Categories.Add(c);
            await db.SaveChangesAsync();
            return c.Id;
        }

        public static async Task SeedActiveTicketForCategoryAsync(AppDbContext db, int categoryId, int requesterId)
        {
            db.Tickets.Add(new TicketEntity
            {
                Title = "T",
                Status = "Novo",
                PriorityLevel = "Média",
                RequesterId = requesterId,
                CategoryId = categoryId
            });

            await db.SaveChangesAsync();
        }

        public static async Task SeedClosedTicketForCategoryAsync(AppDbContext db, int categoryId, int requesterId)
        {
            db.Tickets.Add(new TicketEntity
            {
                Title = "T-Closed",
                Status = "Fechado",
                PriorityLevel = "Média",
                RequesterId = requesterId,
                CategoryId = categoryId
            });

            await db.SaveChangesAsync();
        }

        public static async Task SeedCancelledTicketForCategoryAsync(AppDbContext db, int categoryId, int requesterId)
        {
            db.Tickets.Add(new TicketEntity
            {
                Title = "T-Cancelled",
                Status = "Cancelado",
                PriorityLevel = "Média",
                RequesterId = requesterId,
                CategoryId = categoryId
            });

            await db.SaveChangesAsync();
        }
    }
}