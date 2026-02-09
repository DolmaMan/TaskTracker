using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Services;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Models;
using TaskTracker.Infrastructure.Data;
using Xunit;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Tests;

/// <summary>
/// Unit тесты для ReportService
/// </summary>
public class ReportServiceTests : IDisposable
{
    private readonly TaskTrackerDbContext _context;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskTrackerDbContext(options);
        _reportService = new ReportService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var users = new List<User>
        {
            new User { Id = 1, Name = "User 1", Email = "user1@example.com" },
            new User { Id = 2, Name = "User 2", Email = "user2@example.com" }
        };

        _context.Users.AddRange(users);
        _context.SaveChanges();

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Task 1",
                AssigneeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                DueDate = DateTime.UtcNow.AddDays(5),
                Status = TaskStatus.New,
                Priority = TaskPriority.High
            },
            new TaskItem
            {
                Title = "Task 2",
                AssigneeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(-2), // Overdue
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.Medium
            },
            new TaskItem
            {
                Title = "Task 3",
                AssigneeId = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                DueDate = DateTime.UtcNow.AddDays(-1), // Overdue
                Status = TaskStatus.New,
                Priority = TaskPriority.Low
            },
            new TaskItem
            {
                Title = "Completed Task",
                AssigneeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(-5),
                CompletedAt = DateTime.UtcNow.AddDays(-3),
                Status = TaskStatus.Done,
                Priority = TaskPriority.High
            }
        };

        _context.Tasks.AddRange(tasks);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetStatusSummary_ShouldReturnCorrectCounts()
    {
        // Act
        var result = await _reportService.GetStatusSummaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.StatusCounts["New"]);
        Assert.Equal(1, result.StatusCounts["InProgress"]);
        Assert.Equal(1, result.StatusCounts["Done"]);
        Assert.Equal(2, result.StatusCounts["Overdue"]);
    }

    [Fact]
    public async Task GetOverdueByAssignee_ShouldGroupByAssignee()
    {
        // Act
        var result = await _reportService.GetOverdueByAssigneeAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var user1Report = result.FirstOrDefault(r => r.AssigneeName == "User 1");
        Assert.NotNull(user1Report);
        Assert.Equal(1, user1Report.OverdueCount);

        var user2Report = result.FirstOrDefault(r => r.AssigneeName == "User 2");
        Assert.NotNull(user2Report);
        Assert.Equal(1, user2Report.OverdueCount);
    }

    [Fact]
    public async Task GetAvgCompletionTime_WithCompletedTasks_ShouldCalculateAverage()
    {
        // Act
        var result = await _reportService.GetAvgCompletionTimeAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AverageDays);
        Assert.True(result.AverageDays > 0);
    }

    [Fact]
    public async Task GetAvgCompletionTime_WithoutCompletedTasks_ShouldReturnMessage()
    {
        // Arrange - remove all completed tasks
        var completedTasks = await _context.Tasks
            .Where(t => t.Status == TaskStatus.Done)
            .ToListAsync();
        _context.Tasks.RemoveRange(completedTasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetAvgCompletionTimeAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.AverageDays);
        Assert.Equal("Недостаточно данных для расчёта", result.Message);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}