using Microsoft.EntityFrameworkCore;
using Taskam.Data.Data;
using Taskam.Data.Models;

namespace Taskam.Data.Services
{
    public class TaskService : IDisposable
    {
        private readonly TaskamDbContext _db;
        private bool _disposed;

        public TaskService()
        {
            _db = new TaskamDbContext();
            // Ensure database is created and has all tables
            _db.Database.EnsureCreated();
        }

        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            try
            {
                return await _db.Tasks
                    .OrderBy(t => t.IsCompleted)
                    .ThenByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tasks: {ex.Message}");
                return new List<TaskItem>();
            }
        }

        public async Task<bool> AddTaskAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return false;

            try
            {
                var task = new TaskItem
                {
                    Title = title.Trim(),
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Tasks.Add(task);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding task: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ToggleCompleteAsync(TaskItem task)
        {
            if (task == null) return false;

            try
            {
                var existing = await _db.Tasks.FindAsync(task.Id);
                if (existing == null) return false;

                existing.IsCompleted = !existing.IsCompleted;
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling task: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTaskAsync(TaskItem task)
        {
            if (task == null) return false;

            try
            {
                var existing = await _db.Tasks.FindAsync(task.Id);
                if (existing == null) return false;

                _db.Tasks.Remove(existing);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting task: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetTaskCountAsync()
        {
            try
            {
                return await _db.Tasks.CountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> DeleteAllCompletedTasksAsync()
        {
            try
            {
                var completedTasks = await _db.Tasks.Where(t => t.IsCompleted).ToListAsync();
                _db.Tasks.RemoveRange(completedTasks);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting completed tasks: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _db.Dispose();
                _disposed = true;
            }
        }
    }
}