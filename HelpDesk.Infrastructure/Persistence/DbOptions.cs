namespace HelpDesk.Infrastructure.Persistence;

public sealed class DbOptions
{
    public const string SectionName = "Database";
    public string ConnectionString { get; set; } = string.Empty;
}
