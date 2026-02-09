using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Services;

/// <summary>
/// Интерфейс сервиса для работы с задачами
/// </summary>
public interface ITaskService
{
    Task<List<TaskDto>> GetTasksAsync(string? status = null, int? assigneeId = null,
        DateTime? dueBefore = null, DateTime? dueAfter = null, List<int>? tagIds = null);
    Task<TaskDto?> GetTaskByIdAsync(int id);
    Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto);
    Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto);
    Task<bool> DeleteTaskAsync(int id);
}