using AOP.Logging.Core.Attributes;
using AOP.Logging.Sample.Models;
using Microsoft.Extensions.Logging;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// User service demonstrating sensitive data handling.
/// </summary>
[LogClass]
public partial class UserService : IUserService
{
    private readonly Dictionary<string, User> _users = new();

    /// <summary>
    /// Creates a new user with sensitive password.
    /// </summary>
    public async Task<User> CreateUserAsync(
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
    /// Authenticates a user with sensitive password.
    /// </summary>
    public async Task<bool> AuthenticateAsync(
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
    /// Gets a user by email.
    /// </summary>
    [LogMethod(LogLevel.Debug)]
    public async Task<User?> GetUserByEmailAsync(string email)
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
