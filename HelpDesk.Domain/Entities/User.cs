using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTimeOffset CreatedDt { get; set; }
    public DateTimeOffset UpdatedDt { get; set; }
}
