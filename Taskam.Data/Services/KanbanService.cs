using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Taskam.Data.Data;
using Taskam.Data.Models;

namespace Taskam.Data.Services
{
    public class KanbanService
    {
        private readonly TaskamDbContext _context;

        public KanbanService()
        {
            _context = new TaskamDbContext();
        }

        public KanbanService(TaskamDbContext context)
        {
            _context = context;
        }

        // =========================
        // KANBAN PROJECT CRUD
        // =========================

        public async Task<List<KanbanProject>> GetAllProjectsAsync()
        {
            return await _context.KanbanProjects
                .Include(p => p.BacklogItems)
                .ToListAsync();
        }

        public async Task<KanbanProject?> GetProjectByIdAsync(int id)
        {
            return await _context.KanbanProjects
                .Include(p => p.BacklogItems)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<KanbanProject> CreateProjectAsync(string title, DateTime startDate, DateTime endDate)
        {
            var project = new KanbanProject
            {
                Title = title,
                StartDate = startDate,
                EndDate = endDate,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false,
                Status = "Active"
            };
            _context.KanbanProjects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<KanbanProject?> UpdateProjectAsync(int id, string title, string status, bool isCompleted,
            DateTime startDate, DateTime endDate)   // added dates
        {
            var project = await _context.KanbanProjects.FindAsync(id);
            if (project == null) return null;

            project.Title = title;
            project.Status = status;
            project.IsCompleted = isCompleted;
            project.StartDate = startDate;
            project.EndDate = endDate;

            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.KanbanProjects.FindAsync(id);
            if (project == null) return false;

            _context.KanbanProjects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // BACKLOG CRUD
        // =========================

        public async Task<List<Backlog>> GetAllBacklogsAsync()
        {
            return await _context.Backlogs.ToListAsync();
        }

        public async Task<List<Backlog>> GetBacklogsByProjectAsync(int projectId)
        {
            var project = await _context.KanbanProjects
                .Include(p => p.BacklogItems)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            return project?.BacklogItems.ToList() ?? new List<Backlog>();
        }

        public async Task<Backlog?> GetBacklogByIdAsync(int id)
        {
            return await _context.Backlogs.FindAsync(id);
        }

        public async Task<Backlog?> CreateBacklogAsync(string name, int kanbanProjectId)
        {
            var project = await _context.KanbanProjects.FindAsync(kanbanProjectId);
            if (project == null) return null;

            var backlog = new Backlog
            {
                Name = name,
                CreatedAt = DateTime.UtcNow,
                Status = "Todo",
                KanbanProjectId = kanbanProjectId
            };

            _context.Backlogs.Add(backlog);
            await _context.SaveChangesAsync();
            return backlog;
        }

        public async Task<Backlog?> UpdateBacklogStatusAsync(int id, string status)
        {
            var backlog = await _context.Backlogs.FindAsync(id);
            if (backlog == null) return null;

            backlog.Status = status;
            await _context.SaveChangesAsync();
            return backlog;
        }

        public async Task<Backlog?> UpdateBacklogAsync(int id, string name, string status)
        {
            var backlog = await _context.Backlogs.FindAsync(id);
            if (backlog == null) return null;

            backlog.Name = name;
            backlog.Status = status;

            await _context.SaveChangesAsync();
            return backlog;
        }

        public async Task<bool> DeleteBacklogAsync(int id)
        {
            var backlog = await _context.Backlogs.FindAsync(id);
            if (backlog == null) return false;

            _context.Backlogs.Remove(backlog);
            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // HELPER METHODS
        // =========================

        public async Task<bool> AddBacklogToProjectAsync(int projectId, int backlogId)
        {
            var project = await _context.KanbanProjects
                .Include(p => p.BacklogItems)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            var backlog = await _context.Backlogs.FindAsync(backlogId);

            if (project == null || backlog == null) return false;

            project.BacklogItems.Add(backlog);
            backlog.KanbanProjectId = projectId;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Backlog>> GetBacklogsByStatusAsync(string status)
        {
            return await _context.Backlogs
                .Where(b => b.Status == status)
                .ToListAsync();
        }

        public async Task<bool> RemoveBacklogFromProjectAsync(int projectId, int backlogId)
        {
            var project = await _context.KanbanProjects
                .Include(p => p.BacklogItems)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return false;

            var backlog = project.BacklogItems.FirstOrDefault(b => b.Id == backlogId);
            if (backlog == null) return false;

            project.BacklogItems.Remove(backlog);
            backlog.KanbanProjectId = null;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}