using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Models;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;


namespace TaskTracker.Tests
{
    public class TaskItemOverdueTests 
    {
        private readonly DateTime Now = DateTime.Now;
        
        [Fact]
        public void IsOverdue_DueDateInPast_StatusNew_ReturnsTrue()
        {
            var task = new TaskItem
            {
                DueDate = Now.AddMinutes(-1),
                Status = TaskStatus.New
            };

            Assert.True(task.IsOverdue(Now));
        }

        [Fact]
        public void IsOverdue_DueDateInPast_StatusInProgress_ReturnsTrue()
        {
            var task = new TaskItem
            {
                DueDate = Now.AddDays(-1),
                Status = TaskStatus.InProgress
            };

            Assert.True(task.IsOverdue(Now));
        }

        [Fact]
        public void IsOverdue_DueDateInFuture_StatusNew_ReturnsFalse()
        {
            var task = new TaskItem
            {
                DueDate = Now.AddMinutes(1),
                Status = TaskStatus.New
            };

            Assert.False(task.IsOverdue(Now));
        }

        [Fact]
        public void IsOverdue_StatusDone_EvenIfDueDateInPast_ReturnsFalse()
        {
            var task = new TaskItem
            {
                DueDate = Now.AddDays(-10),
                Status = TaskStatus.Done
            };

            Assert.False(task.IsOverdue(Now));
        }

        // Граничный случай: строгое сравнение "<" ⇒ при равенстве НЕ просрочено
        [Fact]
        public void IsOverdue_DueDateEqualsNow_StatusNotDone_ReturnsFalse()
        {
            var task = new TaskItem
            {
                DueDate = Now,
                Status = TaskStatus.InProgress
            };

            Assert.False(task.IsOverdue(Now));
        }

        // Граничный случай: "на 1 тик раньше" ⇒ просрочено (если не Done)
        [Fact]
        public void IsOverdue_DueDateOneTickBeforeNow_StatusNotDone_ReturnsTrue()
        {
            var task = new TaskItem
            {
                DueDate = Now.AddTicks(-1),
                Status = TaskStatus.New
            };

            Assert.True(task.IsOverdue(Now));
        }
    }
}
