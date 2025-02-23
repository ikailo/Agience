namespace Agience.Authority.Identity.Services
{
    public interface ICrmService
    {
        Task AddSubscriberAsync(string email, string? firstName, string? lastName);
    }
}
