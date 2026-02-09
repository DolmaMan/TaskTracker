namespace TaskTracker.Domain.Models;

/// <summary>
/// Пользователь системы (исполнитель задач)
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email (уникальный)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Задачи, назначенные на пользователя
    /// </summary>
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
}