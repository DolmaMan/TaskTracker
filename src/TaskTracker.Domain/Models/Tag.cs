namespace TaskTracker.Domain.Models;

/// <summary>
/// Тег для категоризации задач
/// </summary>
public class Tag
{
    /// <summary>
    /// Уникальный идентификатор тега
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название тега (bug, feature, refactor, docs)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Связь многие-ко-многим с задачами
    /// </summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}