using HelpDesk.Application.ServiceCatalog.Ports;
using HelpDesk.Domain.ServiceCatalog.ValueObjects;
using HelpDesk.Domain.ServiceCatalog.Aggregates;

namespace HelpDesk.UnitTests.ServiceCatalog.Fakes
{
    public sealed class InMemoryCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories = new();
        private int _nextId = 1;

        public Category Seed(string name, int? parentId = null)
        {
            var cat = CreateWithId(_nextId++, name, parentId);
            _categories.Add(cat);
            return cat;
        }

        private static Category CreateWithId(int id, string name, int? parentId)
        {
            var vo = CategoryName.Create(name);
            var cat = Category.CreateNew(vo, parentId);

            typeof(Category).BaseType!.BaseType!
                .GetProperty("Id")!
                .SetValue(cat, id);

            return cat;
        }

        public Task<bool> NameExistsAsync(string name)
            => Task.FromResult(_categories.Any(c => c.Name.Value == name));

        public Task<Category?> GetByIdAsync(int id)
            => Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));

        public Task<Category?> GetByIdNoTrackingAsync(int id)
            => GetByIdAsync(id);

        public Task<bool> HasChildrenAsync(int id)
            => Task.FromResult(_categories.Any(c => c.ParentId == id));

        public Task AddAsync(Category category)
        {
            typeof(Category).BaseType!.BaseType!
                .GetProperty("Id")!
                .SetValue(category, _nextId++);

            _categories.Add(category);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Category category)
        {
            _categories.RemoveAll(c => c.Id == category.Id);
            return Task.CompletedTask;
        }

        public Task<int> CountAsync(string? nameContains, int? parentId)
        {
            var q = Query(nameContains, parentId);
            return Task.FromResult(q.Count());
        }

        public Task<IReadOnlyList<CategoryListItem>> ListAsync(string? nameContains, int? parentId, int skip, int take)
        {
            var q = Query(nameContains, parentId)
                .OrderBy(c => c.ParentId)
                .ThenBy(c => c.Name.Value)
                .Skip(skip)
                .Take(take);

            var items = q.Select(c =>
            {
                string? parentName = null;
                int? parentCategoryId = null;

                if (c.ParentId.HasValue)
                {
                    var p = _categories.FirstOrDefault(x => x.Id == c.ParentId.Value);
                    parentName = p?.Name.Value;
                    parentCategoryId = p?.Id;
                }

                return new CategoryListItem(
                    c.Id,
                    c.Name.Value,
                    c.ParentId,
                    parentCategoryId,
                    parentName
                );
            }).ToList();

            return Task.FromResult((IReadOnlyList<CategoryListItem>)items);
        }

        private IEnumerable<Category> Query(string? nameContains, int? parentId)
        {
            IEnumerable<Category> q = _categories;

            if (!string.IsNullOrWhiteSpace(nameContains))
                q = q.Where(c => c.Name.Value.Contains(nameContains));

            if (parentId.HasValue)
                q = q.Where(c => c.ParentId == parentId);

            return q;
        }
    }
}
