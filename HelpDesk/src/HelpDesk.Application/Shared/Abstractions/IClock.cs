namespace HelpDesk.Application.Shared.Abstractions
{
    public interface IClock
    {
        DateTime Now { get; }
    }
}
