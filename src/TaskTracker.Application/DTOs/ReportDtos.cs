namespace TaskTracker.Application.DTOs;

/// <summary>
/// DTO для отчёта по статусам
/// </summary>
public class StatusSummaryDto
{
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}

/// <summary>
/// DTO для отчёта по просроченным задачам
/// </summary>
public class OverdueByAssigneeDto
{
    public string AssigneeName { get; set; } = string.Empty;
    public int OverdueCount { get; set; }
    public List<OverdueTaskDto> Tasks { get; set; } = new();
}

public class OverdueTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}

/// <summary>
/// DTO для отчёта по среднему времени закрытия
/// </summary>
public class AvgCompletionTimeDto
{
    public double? AverageDays { get; set; }
    public string Message { get; set; } = string.Empty;
}