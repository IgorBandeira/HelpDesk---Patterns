using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.UnitTests.ServiceCatalog.Fakes
{
    public sealed class FakeTicketQueryPort : ITicketCategoryQueryPort
    {
        private readonly HashSet<int> _categoriesWithActiveTickets = new();

        public void MarkActiveTicketsForCategory(int categoryId)
            => _categoriesWithActiveTickets.Add(categoryId);

        public Task<bool> HasActiveTicketsForCategoryAsync(int categoryId)
            => Task.FromResult(_categoriesWithActiveTickets.Contains(categoryId));
    }
}
