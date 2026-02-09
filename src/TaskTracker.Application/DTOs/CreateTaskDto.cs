using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Application.DTOs;

/// <summary>
/// DTO для создания задачи
/// </summary>
public class CreateTaskDto
{
    /// <summary>
    /// Заголовок задачи (от 3 до 200 символов)
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание задачи (опционально)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ID исполнителя
    /// </summary>
    [Required(ErrorMessage = "AssigneeId is required")]
    public int AssigneeId { get; set; }

    /// <summary>
    /// Дедлайн
    /// </summary>
    [Required(ErrorMessage = "DueDate is required")]
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Приоритет (1-3)
    /// </summary>
    [Range(1, 3, ErrorMessage = "Priority must be between 1 and 3")]
    public int Priority { get; set; } = 2;

    /// <summary>
    /// ID тегов
    /// </summary>
    public List<int> TagIds { get; set; } = new();
}