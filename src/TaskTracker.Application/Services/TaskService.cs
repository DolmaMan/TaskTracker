using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Models;
using TaskTracker.Infrastructure.Data;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Application.Services;

/// <summary>
/// Сервис для работы с задачами
/// </summary>
public class TaskService : ITaskService
{
    private readonly TaskTrackerDbContext _context;

    public TaskService(TaskTrackerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить список задач с фильтрами
    /// </summary>
    public async Task<List<TaskDto>> GetTasksAsync(string? status = null, int? assigneeId = null,
        DateTime? dueBefore = null, DateTime? dueAfter = null, List<int>? tagIds = null)
    {
        var query = _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Tags)
            .AsQueryable();

        // Фильтр по статусу
        if (!string.IsNullOrEmpty(status))
        {
            if (status.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
            {
                var now = DateTime.UtcNow;
                query = query.Where(t => t.DueDate < now && t.Status != TaskStatus.Done);
            }
            else if (Enum.TryParse<TaskStatus>(status, true, out var taskStatus))
            {
                query = query.Where(t => t.Status == taskStatus);
            }
        }

        // Фильтр по исполнителю
        if (assigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == assigneeId.Value);
        }

        // Фильтр по дедлайну
        if (dueBefore.HasValue)
        {
            query = query.Where(t => t.DueDate <= dueBefore.Value);
        }

        if (dueAfter.HasValue)
        {
            query = query.Where(t => t.DueDate >= dueAfter.Value);
        }

        // Фильтр по тегам
        if (tagIds != null && tagIds.Any())
        {
            query = query.Where(t => t.Tags.Any(tag => tagIds.Contains(tag.Id)));
        }

        var tasks = await query.ToListAsync();

        return tasks.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Получить задачу по ID
    /// </summary>
    public async Task<TaskDto?> GetTaskByIdAsync(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id);

        return task == null ? null : MapToDto(task);
    }

    /// <summary>
    /// Создать новую задачу
    /// </summary>
    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto)
    {
        // Проверка существования исполнителя
        var assigneeExists = await _context.Users.AnyAsync(u => u.Id == createTaskDto.AssigneeId);
        if (!assigneeExists)
        {
            throw new InvalidOperationException($"User with ID {createTaskDto.AssigneeId} does not exist");
        }

        var task = new TaskItem
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            AssigneeId = createTaskDto.AssigneeId,
            DueDate = createTaskDto.DueDate,
            Priority = (TaskPriority)createTaskDto.Priority,
            Status = TaskStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        // Добавление тегов
        if (createTaskDto.TagIds.Any())
        {
            var tags = await _context.Tags
                .Where(t => createTaskDto.TagIds.Contains(t.Id))
                .ToListAsync();
            task.Tags = tags;
        }

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return (await GetTaskByIdAsync(task.Id))!;
    }

    /// <summary>
    /// Обновить задачу
    /// </summary>
    public async Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto)
    {
        var task = await _context.Tasks
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return null;
        }

        // Проверка существования исполнителя
        var assigneeExists = await _context.Users.AnyAsync(u => u.Id == updateTaskDto.AssigneeId);
        if (!assigneeExists)
        {
            throw new InvalidOperationException($"User with ID {updateTaskDto.AssigneeId} does not exist");
        }

        task.Title = updateTaskDto.Title;
        task.Description = updateTaskDto.Description;
        task.AssigneeId = updateTaskDto.AssigneeId;
        task.DueDate = updateTaskDto.DueDate;
        task.Status = (TaskStatus)updateTaskDto.Status;
        task.Priority = (TaskPriority)updateTaskDto.Priority;

        // Автоматическое заполнение CompletedAt при переводе в Done
        if (task.Status == TaskStatus.Done && !task.CompletedAt.HasValue)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (task.Status != TaskStatus.Done)
        {
            task.CompletedAt = null;
        }

        // Обновление тегов
        task.Tags.Clear();
        if (updateTaskDto.TagIds.Any())
        {
            var tags = await _context.Tags
                .Where(t => updateTaskDto.TagIds.Contains(t.Id))
                .ToListAsync();
            task.Tags = tags;
        }

        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(task.Id);
    }

    /// <summary>
    /// Удалить задачу
    /// </summary>
    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Маппинг TaskItem в TaskDto
    /// </summary>
    private static TaskDto MapToDto(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            AssigneeId = task.AssigneeId,
            AssigneeName = task.Assignee.Name,
            CreatedAt = task.CreatedAt,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            Status = task.Status.ToString(),
            Priority = (int)task.Priority,
            Tags = task.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList(),
            IsOverdue = task.IsOverdue()
        };
    }
}