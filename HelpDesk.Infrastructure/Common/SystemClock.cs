using HelpDesk.Application.Common;

namespace HelpDesk.Infrastructure.Common;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
