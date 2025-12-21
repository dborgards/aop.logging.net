using AOP.Logging.Core.Attributes;
using AOP.Logging.Sample.Models;
using Microsoft.Extensions.Logging;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// User service demonstrating sensitive data handling.
/// The Source Generator creates public wrapper methods that call these private Core methods.
/// </summary>
[LogClass]
public partial class UserService : IUserService
{
    private readonly Dictionary<string, User> _users = new();

    /// <summary>
    /// Core implementation: Creates a new user with sensitive password.
    /// </summary>
    private async Task<User> CreateUserAsyncCore(
        string email,
        [SensitiveData] string password)
    {
        await Task.Delay(50); // Simulate async work

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        _users[email] = user;
        return user;
    }

    /// <summary>
    /// Core implementation: Authenticates a user with sensitive password.
    /// </summary>
    private async Task<bool> AuthenticateAsyncCore(
        string email,
        [SensitiveData] string password)
    {
        await Task.Delay(50); // Simulate async work

        if (!_users.TryGetValue(email, out var user))
        {
            return false;
        }

        return user.PasswordHash == HashPassword(password);
    }

    /// <summary>
    /// Core implementation: Gets a user by email.
    /// </summary>
    [LogMethod(LogLevel.Debug)]
    private async Task<User?> GetUserByEmailAsyncCore(string email)
    {
        await Task.Delay(20); // Simulate async work
        _users.TryGetValue(email, out var user);
        return user;
    }

    private string HashPassword(string password)
    {
        // Simple hash for demo purposes (NOT for production!)
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }
}
