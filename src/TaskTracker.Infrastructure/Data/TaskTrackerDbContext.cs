using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Models;

namespace TaskTracker.Infrastructure.Data;

/// <summary>
/// Контекст базы данных Task Tracker
/// </summary>
public class TaskTrackerDbContext : DbContext
{
    public TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфигурация User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Конфигурация TaskItem
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.DueDate).IsRequired();

            // Связь с User
            entity.HasOne(e => e.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(e => e.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь многие-ко-многим с Tag
            entity.HasMany(e => e.Tags)
                .WithMany(t => t.Tasks)
                .UsingEntity(j => j.ToTable("TaskTags"));
        });

        // Конфигурация Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Начальные данные (seed data)
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Пользователи
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Иванов Иван", Email = "ivanov@example.com" },
            new User { Id = 2, Name = "Петров Пётр", Email = "petrov@example.com" },
            new User { Id = 3, Name = "Сидорова Анна", Email = "sidorova@example.com" }
        );

        // Теги
        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "bug" },
            new Tag { Id = 2, Name = "feature" },
            new Tag { Id = 3, Name = "refactor" },
            new Tag { Id = 4, Name = "docs" }
        );

        // Примеры задач
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = 1,
                Title = "Исправить ошибку авторизации",
                Description = "Пользователи не могут войти в систему",
                AssigneeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                DueDate = DateTime.UtcNow.AddDays(2),
                Status = Domain.Enums.TaskStatus.InProgress,
                Priority = Domain.Enums.TaskPriority.High
            },
            new TaskItem
            {
                Id = 2,
                Title = "Добавить функцию экспорта в Excel",
                Description = "Реализовать экспорт отчётов в формат Excel",
                AssigneeId = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(7),
                Status = Domain.Enums.TaskStatus.New,
                Priority = Domain.Enums.TaskPriority.Medium
            },
            new TaskItem
            {
                Id = 3,
                Title = "Обновить документацию API",
                Description = "Добавить примеры запросов для всех эндпоинтов",
                AssigneeId = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(-2),
                Status = Domain.Enums.TaskStatus.New,
                Priority = Domain.Enums.TaskPriority.Low
            }
        );
    }
}