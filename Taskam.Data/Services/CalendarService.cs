using Microsoft.EntityFrameworkCore;
using Taskam.Data.Data;
using Taskam.Data.Models;

namespace Taskam.Data.Services
{
    public class CalendarService
    {
        private readonly TaskamDbContext _context;

        public CalendarService()
        {
            _context = new TaskamDbContext();
        }

        public CalendarService(TaskamDbContext context)
        {
            _context = context;
        }

        // Get all events
        public async Task<List<CalendarEvent>> GetEventsAsync()
        {
            return await _context.CalendarEvents
                .OrderBy(e => e.Day)
                .ThenBy(e => e.StartHour)
                .ToListAsync();
        }

        // Get events for specific day
        public async Task<List<CalendarEvent>> GetEventsByDayAsync(int day)
        {
            return await _context.CalendarEvents
                .Where(e => e.Day == day)
                .OrderBy(e => e.StartHour)
                .ToListAsync();
        }

        // Add event
        // Add event - add try-catch
        public async Task<CalendarEvent?> AddEventAsync(string title, int day, int startHour, int durationHours, string color)
        {
            try
            {
                var calendarEvent = new CalendarEvent
                {
                    Title = title,
                    Day = day,
                    StartHour = startHour,
                    DurationHours = durationHours,
                    Color = color
                };

                _context.CalendarEvents.Add(calendarEvent);
                await _context.SaveChangesAsync();

                return calendarEvent;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding event: {ex.Message}");
                return null;
            }
        }

        // Delete event
        public async Task<bool> DeleteEventAsync(int id)
        {
            var ev = await _context.CalendarEvents.FindAsync(id);

            if (ev != null)
            {
                _context.CalendarEvents.Remove(ev);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Update event
        public async Task<bool> UpdateEventAsync(int id, string title, int day, int startHour, int durationHours, string color)
        {
            var ev = await _context.CalendarEvents.FindAsync(id);
            if (ev == null) return false;

            ev.Title = title;
            ev.Day = day;
            ev.StartHour = startHour;
            ev.DurationHours = durationHours;
            ev.Color = color;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}