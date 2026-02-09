using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Services;

namespace TaskTracker.API.Controllers;

/// <summary>
/// Контроллер для управления задачами
/// </summary>
[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Получить список задач с фильтрами
    /// </summary>
    /// <param name="status">Фильтр по статусу (New, InProgress, Done, Overdue)</param>
    /// <param name="assigneeId">Фильтр по ID исполнителя</param>
    /// <param name="dueBefore">Фильтр задач с дедлайном до указанной даты</param>
    /// <param name="dueAfter">Фильтр задач с дедлайном после указанной даты</param>
    /// <param name="tagId">Фильтр по ID тега (можно передать несколько)</param>
    /// <returns>Список задач</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaskDto>), 200)]
    public async Task<ActionResult<List<TaskDto>>> GetTasks(
        [FromQuery] string? status = null,
        [FromQuery] int? assigneeId = null,
        [FromQuery] DateTime? dueBefore = null,
        [FromQuery] DateTime? dueAfter = null,
        [FromQuery] List<int>? tagId = null)
    {
        var tasks = await _taskService.GetTasksAsync(status, assigneeId, dueBefore, dueAfter, tagId);
        return Ok(tasks);
    }

    /// <summary>
    /// Получить задачу по ID
    /// </summary>
    /// <param name="id">ID задачи</param>
    /// <returns>Задача</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound(new { message = "Задача не найдена" });
        }
        return Ok(task);
    }

    /// <summary>
    /// Создать новую задачу
    /// </summary>
    /// <param name="createTaskDto">Данные для создания задачи</param>
    /// <returns>Созданная задача</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = ModelState });
        }

        try
        {
            var task = await _taskService.CreateTaskAsync(createTaskDto);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { errors = new { AssigneeId = new[] { ex.Message } } });
        }
    }

    /// <summary>
    /// Обновить задачу
    /// </summary>
    /// <param name="id">ID задачи</param>
    /// <param name="updateTaskDto">Обновлённые данные задачи</param>
    /// <returns>Обновлённая задача</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = ModelState });
        }

        try
        {
            var task = await _taskService.UpdateTaskAsync(id, updateTaskDto);
            if (task == null)
            {
                return NotFound(new { message = "Задача не найдена" });
            }
            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { errors = new { AssigneeId = new[] { ex.Message } } });
        }
    }

    /// <summary>
    /// Удалить задачу
    /// </summary>
    /// <param name="id">ID задачи</param>
    /// <returns>Результат удаления</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var success = await _taskService.DeleteTaskAsync(id);
        if (!success)
        {
            return NotFound(new { message = "Задача не найдена" });
        }
        return NoContent();
    }
}