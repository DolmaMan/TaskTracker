namespace TaskTracker.Domain.Enums;

/// <summary>
/// Статусы задачи
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Задача создана, но работа не начата
    /// </summary>
    New = 0,

    /// <summary>
    /// Задача в процессе выполнения
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Задача завершена
    /// </summary>
    Done = 2
}