namespace HelpDesk.Application.ServiceCatalog.Ports
{
    public interface ICategoryReadPort
    {
        Task<bool> ExistsAsync(int id);
        Task<string?> GetNameAsync(int id);
    }
}
