using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Ticketing.Models;

namespace HelpDesk.IntegrationTests.Ticketing.Seed
{
    public static class TicketingSeed
    {
        public static async Task<int> SeedTicketAsync(
            AppDbContext db,
            string title,
            string description,
            string status,
            string priority,
            int requesterId,
            int categoryId,
            int? assigneeId = null)
        {
            var t = new TicketEntity
            {
                Title = title,
                Description = description,
                Status = status,
                PriorityLevel = priority,
                CreatedAt = DateTime.Now,
                SlaStartAt = DateTime.Now,
                SlaDueAt = DateTime.Now.AddHours(24),
                RequesterId = requesterId,
                AssigneeId = assigneeId,
                CategoryId = categoryId
            };

            db.Tickets.Add(t);
            await db.SaveChangesAsync();
            return t.Id;
        }
    }
}