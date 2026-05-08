using HelpDesk.Domain.Entities;

namespace HelpDesk.Domain.Repositories;

public interface ITaskRepository
{
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskItem>> ListAsync(Enums.TaskStatus? status, Guid? assignedTo, DateTimeOffset? dueBefore, DateTimeOffset? dueAfter, int page, int pageSize, CancellationToken cancellationToken);
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken);
    Task SoftDeleteAsync(Guid id, DateTimeOffset deletedAt, CancellationToken cancellationToken);
}
