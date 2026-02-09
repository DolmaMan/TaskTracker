using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Application.DTOs;

/// <summary>
/// DTO для обновления задачи
/// </summary>
public class UpdateTaskDto
{
    /// <summary>
    /// Заголовок задачи
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание задачи
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
    /// Статус задачи (0-2)
    /// </summary>
    [Range(0, 2, ErrorMessage = "Status must be between 0 and 2")]
    public int Status { get; set; }

    /// <summary>
    /// Приоритет (1-3)
    /// </summary>
    [Range(1, 3, ErrorMessage = "Priority must be between 1 and 3")]
    public int Priority { get; set; }

    /// <summary>
    /// ID тегов
    /// </summary>
    public List<int> TagIds { get; set; } = new();
}