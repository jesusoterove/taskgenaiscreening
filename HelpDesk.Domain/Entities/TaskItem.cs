namespace HelpDesk.Domain.Entities;

public sealed class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Enums.TaskStatus Status { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public DateTimeOffset CreatedDt { get; set; }
    public DateTimeOffset UpdatedDt { get; set; }
    public DateTimeOffset? DeletedDt { get; set; }
    public Guid AssignedToUserId { get; set; }
}
