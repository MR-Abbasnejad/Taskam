using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using Taskam.Data.Models;

namespace Taskam.Data.Data
{
    public class TaskamDbContext : DbContext
    {
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
        public DbSet<Backlog> Backlogs => Set<Backlog>();
        public DbSet<KanbanProject> KanbanProjects => Set<KanbanProject>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, "taskam.db");

                Console.WriteLine($"[Taskam DB] {dbPath}");

                options.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // BACKLOG CONFIGURATION
            // =========================

            modelBuilder.Entity<Backlog>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(b => b.Status)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasDefaultValue("Todo");

                entity.Property(b => b.CreatedAt)
                      .IsRequired();

                // Add this - configure the foreign key property
                entity.Property(b => b.KanbanProjectId)
                      .IsRequired(false); // Can be null if backlog is not assigned to a project

                entity.HasIndex(b => b.Status);

                // Add this - index for foreign key
                entity.HasIndex(b => b.KanbanProjectId);
            });

            // =========================
            // KANBAN PROJECT CONFIGURATION
            // =========================

            modelBuilder.Entity<KanbanProject>(entity =>
            {
                entity.HasKey(k => k.Id);

                entity.Property(k => k.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(k => k.Status)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasDefaultValue("Active");

                entity.Property(k => k.CreatedAt)
                      .IsRequired();

                entity.Property(k => k.IsCompleted)
                      .IsRequired();

                entity.HasIndex(k => k.Status);

                // Fixed relationship
                entity.HasMany(k => k.BacklogItems)
                      .WithOne(b => b.KanbanProject)
                      .HasForeignKey(b => b.KanbanProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // CALENDAR EVENT CONFIGURATION
            // =========================

            modelBuilder.Entity<CalendarEvent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Color)
                      .HasMaxLength(50);

                entity.Property(e => e.Day)
                      .IsRequired();

                entity.Property(e => e.StartHour)
                      .IsRequired();

                entity.Property(e => e.DurationHours)
                      .IsRequired();

                // Optional indexes for faster queries
                entity.HasIndex(e => e.Day);

                entity.HasIndex(e => new
                {
                    e.Day,
                    e.StartHour
                });
            });
        }
    }
}