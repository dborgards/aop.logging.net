using AOP.Logging.Core.Attributes;

namespace AOP.Logging.Sample.Models;

/// <summary>
/// User model.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;

    [SensitiveData]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
