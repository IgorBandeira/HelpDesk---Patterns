using HelpDesk.Application.ServiceCatalog.Ports;
using HelpDesk.Domain.ServiceCatalog.Aggregates;
using HelpDesk.Domain.ServiceCatalog.ValueObjects;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.ServiceCatalog.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.ServiceCatalog.Repositories
{
    public sealed class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _db;

        public CategoryRepository(AppDbContext db) => _db = db;

        public Task<bool> NameExistsAsync(string name)
        {
            return _db.Set<CategoryEntity>().AnyAsync(x => x.Name == name);
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            var entity = await _db.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == id);
            return entity is null ? null : ToDomain(entity);
        }

        public async Task<Category?> GetByIdNoTrackingAsync(int id)
        {
            var entity = await _db.Set<CategoryEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return entity is null ? null : ToDomain(entity);
        }

        public Task<bool> HasChildrenAsync(int id)
        {
            return _db.Set<CategoryEntity>().AnyAsync(x => x.ParentId == id);
        }

        public async Task AddAsync(Category category)
        {
            var entity = new CategoryEntity
            {
                Name = category.Name.Value,
                ParentId = category.ParentId
            };

            _db.Set<CategoryEntity>().Add(entity);
            await _db.SaveChangesAsync();

            typeof(Category)
                .GetProperty(nameof(Category.Id))!
                .SetValue(category, entity.Id);
        }

        public async Task DeleteAsync(Category category)
        {
            var entity = await _db.Set<CategoryEntity>().FirstAsync(x => x.Id == category.Id);
            _db.Set<CategoryEntity>().Remove(entity);
            await _db.SaveChangesAsync();
        }

        public Task<int> CountAsync(string? nameContains, int? parentId)
        {
            var query = ApplyFilters(_db.Set<CategoryEntity>().AsQueryable(), nameContains, parentId);
            return query.CountAsync();
        }

        public async Task<IReadOnlyList<CategoryListItem>> ListAsync(
     string? nameContains,
     int? parentId,
     int skip,
     int take)
        {
            var query =
                from c in _db.Set<CategoryEntity>().AsNoTracking()
                join p in _db.Set<CategoryEntity>().AsNoTracking()
                    on c.ParentId equals p.Id into parentJoin
                from parent in parentJoin.DefaultIfEmpty()
                select new
                {
                    c.Id,
                    c.Name,
                    c.ParentId,
                    ParentCategoryId = parent != null ? (int?)parent.Id : null,
                    ParentName = parent != null ? parent.Name : null
                };

            if (!string.IsNullOrWhiteSpace(nameContains))
            {
                query = query.Where(x => x.Name.Contains(nameContains));
            }

            if (parentId.HasValue)
            {
                query = query.Where(x => x.ParentId == parentId.Value);
            }

            var items = await query
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return items
                .Select(x => new CategoryListItem(
                    x.Id,
                    x.Name,
                    x.ParentId,
                    x.ParentCategoryId,
                    x.ParentName))
                .ToList();
        }

        private static IQueryable<CategoryEntity> ApplyFilters(
            IQueryable<CategoryEntity> query,
            string? nameContains,
            int? parentId)
        {
            if (!string.IsNullOrWhiteSpace(nameContains))
                query = query.Where(x => x.Name.Contains(nameContains));

            if (parentId.HasValue)
                query = query.Where(x => x.ParentId == parentId.Value);

            return query;
        }

        private static Category ToDomain(CategoryEntity entity)
        {
            var name = CategoryName.Create(entity.Name);

            return typeof(Category)
                .GetMethod("CreateNew")!
                .Invoke(null, new object?[] { name, entity.ParentId }) is Category created
                ? SetId(created, entity.Id)
                : throw new InvalidOperationException("Não foi possível materializar Category.");
        }

        private static Category SetId(Category category, int id)
        {
            typeof(Category)
                .GetProperty(nameof(Category.Id))!
                .SetValue(category, id);

            return category;
        }
    }
}
