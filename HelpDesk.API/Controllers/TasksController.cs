using HelpDesk.API.Extensions;
using HelpDesk.Application.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var actor = HttpContext.ToActor();
        var created = await _taskService.CreateTaskAsync(request, actor, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> List([FromQuery] string? status, [FromQuery] Guid? assignedTo, [FromQuery] DateTimeOffset? dueBefore, [FromQuery] DateTimeOffset? dueAfter, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        HelpDesk.Domain.Enums.TaskStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<HelpDesk.Domain.Enums.TaskStatus>(status, true, out var value))
            {
                return BadRequest("Invalid status value.");
            }

            parsedStatus = value;
        }

        var actor = HttpContext.ToActor();
        var tasks = await _taskService.ListTasksAsync(new TaskListQuery(parsedStatus, assignedTo, dueBefore, dueAfter, page, pageSize), actor, cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var actor = HttpContext.ToActor();
        return Ok(await _taskService.GetTaskByIdAsync(id, actor, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var actor = HttpContext.ToActor();
        return Ok(await _taskService.UpdateTaskAsync(id, request, actor, cancellationToken));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Patch(Guid id, PatchTaskRequest request, CancellationToken cancellationToken)
    {
        var actor = HttpContext.ToActor();
        return Ok(await _taskService.PatchTaskAsync(id, request, actor, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var actor = HttpContext.ToActor();
        await _taskService.DeleteTaskAsync(id, actor, cancellationToken);
        return NoContent();
    }
}
