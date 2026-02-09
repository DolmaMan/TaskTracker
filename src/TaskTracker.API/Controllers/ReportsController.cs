using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Services;

namespace TaskTracker.API.Controllers;

/// <summary>
/// Контроллер для получения отчётов
/// </summary>
[ApiController]
[Route("api/reports")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Получить отчёт по количеству задач в каждом статусе
    /// </summary>
    /// <returns>Количество задач по статусам</returns>
    [HttpGet("status-summary")]
    [ProducesResponseType(typeof(StatusSummaryDto), 200)]
    public async Task<ActionResult<StatusSummaryDto>> GetStatusSummary()
    {
        var report = await _reportService.GetStatusSummaryAsync();
        return Ok(report);
    }

    /// <summary>
    /// Получить отчёт по просроченным задачам по исполнителям
    /// </summary>
    /// <returns>Список просроченных задач по исполнителям</returns>
    [HttpGet("overdue-by-assignee")]
    [ProducesResponseType(typeof(List<OverdueByAssigneeDto>), 200)]
    public async Task<ActionResult<List<OverdueByAssigneeDto>>> GetOverdueByAssignee()
    {
        var report = await _reportService.GetOverdueByAssigneeAsync();
        return Ok(report);
    }

    /// <summary>
    /// Получить отчёт по среднему времени закрытия задач
    /// </summary>
    /// <returns>Среднее время закрытия в днях</returns>
    [HttpGet("avg-completion-time")]
    [ProducesResponseType(typeof(AvgCompletionTimeDto), 200)]
    public async Task<ActionResult<AvgCompletionTimeDto>> GetAvgCompletionTime()
    {
        var report = await _reportService.GetAvgCompletionTimeAsync();
        return Ok(report);
    }
}