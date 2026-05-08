namespace HelpDesk.Application.Tasks;

public sealed record CreateTaskRequest(string Title, string? Description, DateTimeOffset DueDate, HelpDesk.Domain.Enums.TaskStatus? Status, Guid? UserId);
public sealed record UpdateTaskRequest(string Title, string? Description, DateTimeOffset DueDate, HelpDesk.Domain.Enums.TaskStatus? Status, Guid? AssignedToUserId);
public sealed record PatchTaskRequest(string? Title, string? Description, DateTimeOffset? DueDate, HelpDesk.Domain.Enums.TaskStatus? Status, Guid? AssignedToUserId);
public sealed record TaskListQuery(HelpDesk.Domain.Enums.TaskStatus? Status, Guid? AssignedTo, DateTimeOffset? DueBefore, DateTimeOffset? DueAfter, int Page, int PageSize);
public sealed record TaskResponse(Guid Id, string Title, string? Description, string Status, DateTimeOffset DueDate, DateTimeOffset CreatedDt, DateTimeOffset UpdatedDt, Guid AssignedToUserId);
