using HelpDesk.Infrastructure.Attachments.Models;
using HelpDesk.Infrastructure.IdentityAccess.Models;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.ServiceCatalog.Models;
using HelpDesk.Infrastructure.Ticketing.Models;

namespace HelpDesk.IntegrationTests.Attachments.Fixtures
{
    public static class AttachmentsSeed
    {
        public static async Task<UserEntity> SeedUserAsync(
            AppDbContext db,
            string name,
            string email,
            string role = "Requester")
        {
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
            string? name = null)
        {
            var category = new CategoryEntity
            {
                Name = name ?? $"Infra-{Guid.NewGuid():N}"
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return category;
        }

        public static async Task<TicketEntity> SeedTicketAsync(
            AppDbContext db,
            int requesterId,
            int categoryId,
            string status = "Aberto")
        {
            var ticket = new TicketEntity
            {
                Title = "Ticket teste",
                Description = "Descrição teste",
                Status = status,
                PriorityLevel = "Media",
                RequesterId = requesterId,
                CategoryId = categoryId,
                CreatedAt = DateTime.UtcNow,
                SlaStartAt = DateTime.UtcNow
            };

            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();
            return ticket;
        }

        public static async Task<AttachmentEntity> SeedAttachmentAsync(
            AppDbContext db,
            int ticketId,
            int uploadedById,
            string fileName = "arquivo.pdf",
            string contentType = "application/pdf",
            long sizeBytes = 1024,
            string? storageKey = null)
        {
            var attachment = new AttachmentEntity
            {
                TicketId = ticketId,
                FileName = fileName,
                ContentType = contentType,
                SizeBytes = sizeBytes,
                StorageKey = storageKey ?? $"tickets/{ticketId}/{fileName}",
                PublicUrl = $"https://fake-storage.local/tickets/{ticketId}/{fileName}",
                UploadedById = uploadedById,
                UploadedAt = DateTime.UtcNow
            };

            db.Attachments.Add(attachment);
            await db.SaveChangesAsync();
            return attachment;
        }
    }
}