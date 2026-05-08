namespace HelpDesk.Application.Common;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
