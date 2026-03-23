using HelpDesk.Application.ServiceCatalog.Ports;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.ServiceCatalog.Queries
{
    public sealed class CategoryReadPort : ICategoryReadPort
    {
        private readonly AppDbContext _db;

        public CategoryReadPort(AppDbContext db) => _db = db;

        public Task<bool> ExistsAsync(int id)
        {
            return _db.Set<Models.CategoryEntity>()
                .AnyAsync(x => x.Id == id);
        }

        public async Task<string?> GetNameAsync(int id)
        {
            return await _db.Set<Models.CategoryEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();
        }
    }
}