using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Enums;
using TaskTracker.Infrastructure.Data;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Application.Services;

/// <summary>
/// Сервис для генерации отчётов
/// </summary>
public class ReportService : IReportService
{
    private readonly TaskTrackerDbContext _context;

    public ReportService(TaskTrackerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отчёт по количеству задач в каждом статусе
    /// </summary>
    public async Task<StatusSummaryDto> GetStatusSummaryAsync()
    {
        var tasks = await _context.Tasks.ToListAsync();
        var now = DateTime.UtcNow;

        var statusCounts = new Dictionary<string, int>
        {
            ["New"] = tasks.Count(t => t.Status == TaskStatus.New),
            ["InProgress"] = tasks.Count(t => t.Status == TaskStatus.InProgress),
            ["Done"] = tasks.Count(t => t.Status == TaskStatus.Done),
            ["Overdue"] = tasks.Count(t => t.DueDate < now && t.Status != TaskStatus.Done)
        };

        return new StatusSummaryDto { StatusCounts = statusCounts };
    }

    /// <summary>
    /// Отчёт по просроченным задачам по исполнителям
    /// </summary>
    public async Task<List<OverdueByAssigneeDto>> GetOverdueByAssigneeAsync()
    {
        var now = DateTime.UtcNow;

        var overdueTasks = await _context.Tasks
            .Include(t => t.Assignee)
            .Where(t => t.DueDate < now && t.Status != TaskStatus.Done)
            .ToListAsync();

        var grouped = overdueTasks
            .GroupBy(t => new { t.AssigneeId, t.Assignee.Name })
            .Select(g => new OverdueByAssigneeDto
            {
                AssigneeName = g.Key.Name,
                OverdueCount = g.Count(),
                Tasks = g.Select(t => new OverdueTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate
                }).ToList()
            })
            .OrderByDescending(x => x.OverdueCount)
            .ToList();

        return grouped;
    }

    /// <summary>
    /// Отчёт по среднему времени закрытия задач
    /// </summary>
    public async Task<AvgCompletionTimeDto> GetAvgCompletionTimeAsync()
    {
        var completedTasks = await _context.Tasks
            .Where(t => t.Status == TaskStatus.Done && t.CompletedAt.HasValue)
            .ToListAsync();

        if (!completedTasks.Any())
        {
            return new AvgCompletionTimeDto
            {
                AverageDays = null,
                Message = "Недостаточно данных для расчёта"
            };
        }

        var avgDays = completedTasks
            .Select(t => (t.CompletedAt!.Value - t.CreatedAt).TotalDays)
            .Average();

        return new AvgCompletionTimeDto
        {
            AverageDays = Math.Round(avgDays, 2),
            Message = $"Среднее время закрытия задачи: {Math.Round(avgDays, 2)} дней"
        };
    }
}