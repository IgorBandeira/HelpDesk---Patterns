using HelpDesk.Application.ServiceCatalog.Ports;

namespace HelpDesk.UnitTests.Ticketing.Fakes
{
    public sealed class InMemoryCategoryReadPort : ICategoryReadPort
    {
        private readonly Dictionary<int, string> _cats = new();

        public void Seed(int id, string name) => _cats[id] = name;

        public Task<bool> ExistsAsync(int id) => Task.FromResult(_cats.ContainsKey(id));

        public Task<string?> GetNameAsync(int id)
            => Task.FromResult(_cats.TryGetValue(id, out var n) ? n : null);
    }
}
