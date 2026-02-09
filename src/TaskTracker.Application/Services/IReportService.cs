using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Services;

/// <summary>
/// Интерфейс сервиса отчётов
/// </summary>
public interface IReportService
{
    Task<StatusSummaryDto> GetStatusSummaryAsync();
    Task<List<OverdueByAssigneeDto>> GetOverdueByAssigneeAsync();
    Task<AvgCompletionTimeDto> GetAvgCompletionTimeAsync();
}