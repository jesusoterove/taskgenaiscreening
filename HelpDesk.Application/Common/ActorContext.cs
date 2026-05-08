using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Common;

public sealed class ActorContext
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required UserRole Role { get; init; }
}
