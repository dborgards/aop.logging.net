using AOP.Logging.Sample.Models;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// User service interface.
/// </summary>
public interface IUserService
{
    Task<User> CreateUserAsync(string email, string password);
    Task<bool> AuthenticateAsync(string email, string password);
    Task<User?> GetUserByEmailAsync(string email);
}
