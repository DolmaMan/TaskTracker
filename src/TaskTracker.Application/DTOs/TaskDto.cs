using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs;

/// <summary>
/// DTO для отображения задачи
/// </summary>
public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssigneeId { get; set; }
    public string AssigneeName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public List<TagDto> Tags { get; set; } = new();
    public bool IsOverdue { get; set; }
}

/// <summary>
/// DTO для тега
/// </summary>
public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}