using TaskTracker.Domain.Enums;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Domain.Models;

/// <summary>
/// Задача в системе Task Tracker
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Уникальный идентификатор задачи
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Заголовок задачи (обязательное поле)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание задачи (опционально)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ID исполнителя (ссылка на Users.Id)
    /// </summary>
    public int AssigneeId { get; set; }

    /// <summary>
    /// Исполнитель задачи
    /// </summary>
    public User Assignee { get; set; } = null!;

    /// <summary>
    /// Дата и время создания
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дедлайн (срок выполнения)
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Дата закрытия (null, если не завершена)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Статус задачи: New, InProgress, Done
    /// </summary>
    public TaskStatus Status { get; set; }

    /// <summary>
    /// Приоритет: Low(1), Medium(2), High(3)
    /// </summary>
    public TaskPriority Priority { get; set; }

    /// <summary>
    /// Теги задачи
    /// </summary>
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    /// <summary>
    /// Проверяет, просрочена ли задача
    /// </summary>
    public bool IsOverdue(DateTime nowUtc)
    {
        return DueDate < nowUtc && Status != TaskStatus.Done;
    }
}