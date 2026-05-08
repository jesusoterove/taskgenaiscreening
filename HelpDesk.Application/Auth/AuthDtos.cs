using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Auth;

public sealed record LoginRequest(string Email, string Password);
public sealed record RegisterRequest(string Email, string Name, string Password, UserRole Role);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record AuthResponse(Guid UserId, string Email, string Name, string Role, string Token);
