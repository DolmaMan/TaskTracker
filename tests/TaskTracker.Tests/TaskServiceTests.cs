using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Services;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Models;
using TaskTracker.Infrastructure.Data;
using Xunit;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Tests;

/// <summary>
/// Unit тесты для TaskService
/// </summary>
public class TaskServiceTests : IDisposable
{
    private readonly TaskTrackerDbContext _context;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskTrackerDbContext(options);
        _taskService = new TaskService(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var users = new List<User>
        {
            new User { Id = 1, Name = "Test User 1", Email = "test1@example.com" },
            new User { Id = 2, Name = "Test User 2", Email = "test2@example.com" }
        };

        var tags = new List<Tag>
        {
            new Tag { Id = 1, Name = "bug" },
            new Tag { Id = 2, Name = "feature" }
        };

        _context.Users.AddRange(users);
        _context.Tags.AddRange(tags);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateTask_ShouldCreateTaskWithNewStatus()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            AssigneeId = 1,
            DueDate = DateTime.UtcNow.AddDays(5),
            Priority = 2,
            TagIds = new List<int> { 1 }
        };

        // Act
        var result = await _taskService.CreateTaskAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("New", result.Status);
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task UpdateTask_ToStatusDone_ShouldSetCompletedAt()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Test Task",
            AssigneeId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            DueDate = DateTime.UtcNow.AddDays(2),
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Medium
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Task",
            AssigneeId = 1,
            DueDate = task.DueDate,
            Status = 2, // Done
            Priority = 2,
            TagIds = new List<int>()
        };

        // Act
        var result = await _taskService.UpdateTaskAsync(task.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Done", result.Status);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task GetTasks_WithStatusFilter_ShouldReturnFilteredTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Task 1",
                AssigneeId = 1,
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(5),
                Status = TaskStatus.New,
                Priority = TaskPriority.High
            },
            new TaskItem
            {
                Title = "Task 2",
                AssigneeId = 1,
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(5),
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.Medium
            }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetTasksAsync(status: "New");

        // Assert
        Assert.Single(result);
        Assert.Equal("New", result[0].Status);
    }

    [Fact]
    public async Task GetTasks_WithOverdueFilter_ShouldReturnOverdueTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Overdue Task",
                AssigneeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(-2), // Просрочено
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High
            },
            new TaskItem
            {
                Title = "Active Task",
                AssigneeId = 1,
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(5),
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.Medium
            }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetTasksAsync(status: "Overdue");

        // Assert
        Assert.Single(result);
        Assert.True(result[0].IsOverdue);
    }

    [Fact]
    public async Task DeleteTask_ShouldRemoveTaskFromDatabase()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Task to Delete",
            AssigneeId = 1,
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(5),
            Status = TaskStatus.New,
            Priority = TaskPriority.Low
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        var taskId = task.Id;

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId);

        // Assert
        Assert.True(result);
        var deletedTask = await _taskService.GetTaskByIdAsync(taskId);
        Assert.Null(deletedTask);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}