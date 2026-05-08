using HelpDesk.Application.Common;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Repositories;

namespace HelpDesk.Application.Tasks;

public sealed class TaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClock _clock;

    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository, IClock clock)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _clock = clock;
    }

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, ActorContext actor, CancellationToken cancellationToken)
    {
        ValidateTitle(request.Title);
        ValidateDescription(request.Description);
        ValidateDueDate(request.DueDate, _clock.UtcNow);

        var ownerId = request.UserId ?? actor.UserId;
        if (ownerId != actor.UserId && actor.Role != UserRole.Admin)
        {
            throw new AppException("Only admin users can create tasks on behalf of another user.", 403);
        }

        if (await _userRepository.GetByIdAsync(ownerId, cancellationToken) is null)
        {
            throw new AppException("Target user was not found.", 404);
        }

        var now = _clock.UtcNow;
        var entity = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = NormalizeDescription(request.Description),
            Status = request.Status ?? HelpDesk.Domain.Enums.TaskStatus.Open,
            DueDate = request.DueDate.ToUniversalTime(),
            CreatedDt = now,
            UpdatedDt = now,
            AssignedToUserId = ownerId
        };

        var created = await _taskRepository.CreateAsync(entity, cancellationToken);
        return ToResponse(created);
    }

    public async Task<TaskResponse> GetTaskByIdAsync(Guid taskId, ActorContext actor, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken)
                   ?? throw new AppException("Task was not found.", 404);

        EnsureCanAccessTask(task, actor);
        return ToResponse(task);
    }

    public async Task<IReadOnlyList<TaskResponse>> ListTasksAsync(TaskListQuery query, ActorContext actor, CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize is <= 0 or > 100 ? 20 : query.PageSize;
        var assignedTo = actor.Role == UserRole.Admin ? query.AssignedTo : actor.UserId;

        var tasks = await _taskRepository.ListAsync(query.Status, assignedTo, query.DueBefore, query.DueAfter, page, pageSize, cancellationToken);

        if (actor.Role != UserRole.Admin)
        {
            tasks = tasks.Where(x => x.AssignedToUserId == actor.UserId).ToList();
        }

        return tasks.Select(ToResponse).ToList();
    }

    public async Task<TaskResponse> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, ActorContext actor, CancellationToken cancellationToken)
    {
        ValidateTitle(request.Title);
        ValidateDescription(request.Description);
        ValidateDueDate(request.DueDate, _clock.UtcNow);

        var existing = await _taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new AppException("Task was not found.", 404);

        EnsureCanEditTask(existing, actor);

        if (request.AssignedToUserId.HasValue && request.AssignedToUserId.Value != existing.AssignedToUserId)
        {
            EnsureAdmin(actor);
            if (await _userRepository.GetByIdAsync(request.AssignedToUserId.Value, cancellationToken) is null)
            {
                throw new AppException("Assigned user was not found.", 404);
            }

            existing.AssignedToUserId = request.AssignedToUserId.Value;
        }

        if (request.Status.HasValue && actor.UserId != existing.AssignedToUserId)
        {
            throw new AppException("Only task owner can change task status.", 403);
        }

        existing.Title = request.Title.Trim();
        existing.Description = NormalizeDescription(request.Description);
        existing.DueDate = request.DueDate.ToUniversalTime();
        existing.Status = request.Status ?? existing.Status;
        existing.UpdatedDt = _clock.UtcNow;

        await _taskRepository.UpdateAsync(existing, cancellationToken);
        return ToResponse(existing);
    }

    public async Task<TaskResponse> PatchTaskAsync(Guid taskId, PatchTaskRequest request, ActorContext actor, CancellationToken cancellationToken)
    {
        var existing = await _taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new AppException("Task was not found.", 404);

        EnsureCanEditTask(existing, actor);

        if (request.Title is not null)
        {
            ValidateTitle(request.Title);
            existing.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            ValidateDescription(request.Description);
            existing.Description = NormalizeDescription(request.Description);
        }

        if (request.DueDate.HasValue)
        {
            ValidateDueDate(request.DueDate.Value, _clock.UtcNow);
            existing.DueDate = request.DueDate.Value.ToUniversalTime();
        }

        if (request.Status.HasValue)
        {
            if (actor.UserId != existing.AssignedToUserId)
            {
                throw new AppException("Only task owner can change task status.", 403);
            }

            existing.Status = request.Status.Value;
        }

        if (request.AssignedToUserId.HasValue && request.AssignedToUserId.Value != existing.AssignedToUserId)
        {
            EnsureAdmin(actor);
            if (await _userRepository.GetByIdAsync(request.AssignedToUserId.Value, cancellationToken) is null)
            {
                throw new AppException("Assigned user was not found.", 404);
            }

            existing.AssignedToUserId = request.AssignedToUserId.Value;
        }

        existing.UpdatedDt = _clock.UtcNow;
        await _taskRepository.UpdateAsync(existing, cancellationToken);
        return ToResponse(existing);
    }

    public async Task DeleteTaskAsync(Guid taskId, ActorContext actor, CancellationToken cancellationToken)
    {
        var existing = await _taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new AppException("Task was not found.", 404);

        EnsureCanEditTask(existing, actor);
        await _taskRepository.SoftDeleteAsync(taskId, _clock.UtcNow, cancellationToken);
    }

    private static TaskResponse ToResponse(TaskItem task) => new(task.Id, task.Title, task.Description, task.Status.ToString(), task.DueDate, task.CreatedDt, task.UpdatedDt, task.AssignedToUserId);

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200)
        {
            throw new AppException("Title is required and must be 200 characters or fewer.", 400);
        }
    }

    private static void ValidateDescription(string? description)
    {
        if (description?.Length > 2000)
        {
            throw new AppException("Description must be 2000 characters or fewer.", 400);
        }
    }

    private static void ValidateDueDate(DateTimeOffset dueDate, DateTimeOffset now)
    {
        if (dueDate <= now)
        {
            throw new AppException("DueDate must be in the future.", 400);
        }
    }

    private static string? NormalizeDescription(string? description) => string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static void EnsureCanAccessTask(TaskItem task, ActorContext actor)
    {
        if (actor.Role != UserRole.Admin && task.AssignedToUserId != actor.UserId)
        {
            throw new AppException("You do not have permission to access this task.", 403);
        }
    }

    private static void EnsureCanEditTask(TaskItem task, ActorContext actor)
    {
        if (actor.Role != UserRole.Admin && task.AssignedToUserId != actor.UserId)
        {
            throw new AppException("You do not have permission to modify this task.", 403);
        }
    }

    private static void EnsureAdmin(ActorContext actor)
    {
        if (actor.Role != UserRole.Admin)
        {
            throw new AppException("This action requires Admin role.", 403);
        }
    }
}
