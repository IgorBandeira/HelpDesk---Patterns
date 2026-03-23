using HelpDesk.Infrastructure.Collaboration.Models;
using HelpDesk.Infrastructure.IdentityAccess.Models;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.ServiceCatalog.Models;
using HelpDesk.Infrastructure.Ticketing.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.IntegrationTests.Collaboration.Fixtures
{
    public static class CollaborationSeed
    {
        public static async Task<UserEntity> SeedUserAsync(
            AppDbContext db,
            string name,
            string email,
            string role)
        {
            var existing = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (existing is not null)
                return existing;

            var user = new UserEntity
            {
                Name = name,
                Email = email,
                Role = role
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

        public static async Task<CategoryEntity> SeedCategoryAsync(
            AppDbContext db,
            string name = "Categoria Teste")
        {
            var existing = await db.Set<CategoryEntity>()
                .FirstOrDefaultAsync(x => x.Name == name);

            if (existing is not null)
                return existing;

            var category = new CategoryEntity
            {
                Name = name
            };

            db.Set<CategoryEntity>().Add(category);
            await db.SaveChangesAsync();
            return category;
        }

        public static async Task<TicketEntity> SeedTicketAsync(
            AppDbContext db,
            string title,
            string description,
            string status,
            int requesterId,
            int? assigneeId = null,
            int? categoryId = null,
            string priority = "Média")
        {
            if (categoryId is null)
            {
                var category = await SeedCategoryAsync(db, "Categoria Teste");
                categoryId = category.Id;
            }

            var now = DateTime.UtcNow;

            var ticket = new TicketEntity
            {
                Title = title,
                Description = description,
                Status = status,
                PriorityLevel = priority,
                CreatedAt = now,
                SlaStartAt = now,
                SlaDueAt = now.AddHours(48),
                RequesterId = requesterId,
                AssigneeId = assigneeId,
                CategoryId = categoryId.Value
            };

            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();
            return ticket;
        }

        public static async Task<CommentEntity> SeedCommentAsync(
            AppDbContext db,
            int ticketId,
            int? authorId,
            string visibility,
            string message)
        {
            var comment = new CommentEntity
            {
                TicketId = ticketId,
                AuthorId = authorId,
                Visibility = visibility,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            db.Set<CommentEntity>().Add(comment);
            await db.SaveChangesAsync();
            return comment;
        }
    }
}