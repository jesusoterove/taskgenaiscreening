using System.Security.Claims;
using HelpDesk.Application.Common;
using HelpDesk.Domain.Enums;

namespace HelpDesk.API.Extensions;

public static class HttpContextExtensions
{
    public static ActorContext ToActor(this HttpContext httpContext)
    {
        var principal = httpContext.User;
        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? principal.FindFirstValue("sub")
                  ?? throw new InvalidOperationException("Missing sub claim.");

        var email = principal.FindFirstValue(ClaimTypes.Email)
                    ?? principal.FindFirstValue("email")
                    ?? string.Empty;

        var roleString = principal.FindFirstValue(ClaimTypes.Role)
                         ?? principal.FindFirstValue("role")
                         ?? UserRole.User.ToString();

        Enum.TryParse<UserRole>(roleString, true, out var role);

        return new ActorContext
        {
            UserId = Guid.Parse(sub),
            Email = email,
            Role = role
        };
    }
}
