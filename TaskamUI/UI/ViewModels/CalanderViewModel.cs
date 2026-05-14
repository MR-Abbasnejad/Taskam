using System.Collections.ObjectModel;
using Taskam.Data.Models;
using Taskam.Data.Services;

namespace TaskamUI.UI.ViewModels
{
    public class CalendarViewModel
    {
        private readonly CalendarService _calendarService;
        private readonly SystemTimeService _timeService;

        public ObservableCollection<CalendarEvent> Events { get; } = new();

        public DateTime CurrentDate => _timeService.CurrentDate;
        public TimeSpan CurrentTime => _timeService.CurrentTime;

        public CalendarViewModel(
            CalendarService calendarService,
            SystemTimeService timeService)
        {
            _calendarService = calendarService;
            _timeService = timeService;

            _timeService.PropertyChanged += (_, __) =>
            {
                // UI can react to time changes if needed
            };
        }

        public async Task LoadEventsAsync()
        {
            Events.Clear();

            var events = await _calendarService.GetEventsAsync();

            foreach (var ev in events)
                Events.Add(ev);
        }

        // Fixed: Match the service method signature
        public async Task AddEventAsync(string title, int day, int startHour, int durationHours, string color)
        {
            var created = await _calendarService.AddEventAsync(title, day, startHour, durationHours, color);
            if (created != null)
                Events.Add(created);
        }

        // Optional: Overload that accepts CalendarEvent
        public async Task AddEventAsync(CalendarEvent ev)
        {
            var created = await _calendarService.AddEventAsync(ev.Title, ev.Day, ev.StartHour, ev.DurationHours, ev.Color);
            if (created != null)
                Events.Add(created);
        }

        public async Task DeleteEventAsync(CalendarEvent ev)
        {
            await _calendarService.DeleteEventAsync(ev.Id);
            Events.Remove(ev);
        }
    }
}