using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Taskam.Data.Models;
using Taskam.Data.Services;

namespace TaskamUI.UI.ViewModels
{
    public class TimelineViewModel : INotifyPropertyChanged
    {
        private readonly KanbanService _kanbanService;
        private readonly SystemTimeService _timeService;

        private const double CellWidth = 80.0;
        private const double RowHeight = 50.0;

        public ObservableCollection<KanbanProject> Projects { get; } = new();
        public ObservableCollection<DateTime> Dates { get; } = new();

        public DateTime Today => _timeService.CurrentDate;
        public double TimelineWidth => Dates.Count * CellWidth;
        public DateTime TimelineStart => Dates.Count > 0 ? Dates[0] : Today;

        public TimelineViewModel()
        {
            _kanbanService = new KanbanService();
            _timeService = new SystemTimeService();
            _timeService.PropertyChanged += OnTimeServicePropertyChanged;
            _ = LoadProjectsAsync();
        }

        public async Task AddProjectAsync(string name, DateTime start, DateTime end)
        {
            var newProject = await _kanbanService.CreateProjectAsync(name, start, end);
            Projects.Add(newProject);
            GenerateDatesRange();
            OnPropertyChanged(nameof(TimelineWidth));
            OnPropertyChanged(nameof(TimelineStart));
            OnPropertyChanged(nameof(TodayX));
        }

        public async Task DeleteProjectAsync(KanbanProject project)
        {
            if (project == null) return;

            var result = MessageBox.Show($"Delete project '{project.Title}' and all its tasks?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _kanbanService.DeleteProjectAsync(project.Id);
                Projects.Remove(project);
                GenerateDatesRange();
                OnPropertyChanged(nameof(TimelineWidth));
                OnPropertyChanged(nameof(TimelineStart));
                OnPropertyChanged(nameof(TodayX));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete project: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadProjectsAsync()
        {
            var allProjects = await _kanbanService.GetAllProjectsAsync();
            Projects.Clear();
            foreach (var project in allProjects)
                Projects.Add(project);
            GenerateDatesRange();
            OnPropertyChanged(nameof(TimelineWidth));
            OnPropertyChanged(nameof(TimelineStart));
            OnPropertyChanged(nameof(TodayX));
        }

        private void GenerateDatesRange()
        {
            Dates.Clear();
            DateTime minDate = Today;
            DateTime maxDate = Today;
            foreach (var project in Projects)
            {
                if (project.StartDate < minDate) minDate = project.StartDate;
                if (project.EndDate > maxDate) maxDate = project.EndDate;
            }
            DateTime start = minDate.AddDays(-5);
            DateTime end = maxDate.AddDays(5);
            int totalDays = (int)(end - start).TotalDays;
            if (totalDays < 30) totalDays = 30;
            for (int i = 0; i <= totalDays; i++)
                Dates.Add(start.AddDays(i));
        }

        public double GetLeftOffset(DateTime projectStart)
        {
            int dayIndex = (projectStart - TimelineStart).Days;
            if (dayIndex < 0) dayIndex = 0;
            if (dayIndex >= Dates.Count) dayIndex = Dates.Count - 1;
            return dayIndex * CellWidth;
        }

        public double GetWidth(DateTime start, DateTime end)
        {
            int durationDays = (end - start).Days + 1;
            if (durationDays < 1) durationDays = 1;
            return durationDays * CellWidth;
        }

        public double GetTopOffset(int index) => index * RowHeight + 11;

        public double TodayX
        {
            get
            {
                var todayDate = Today.Date;
                int index = Dates.IndexOf(todayDate);
                if (index >= 0) return index * CellWidth;
                if (Dates.Count == 0) return 0;
                if (todayDate < Dates[0]) return 0;
                if (todayDate > Dates[^1]) return (Dates.Count - 1) * CellWidth;
                return 0;
            }
        }

        private void OnTimeServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemTimeService.CurrentDate))
            {
                GenerateDatesRange();
                OnPropertyChanged(nameof(Today));
                OnPropertyChanged(nameof(TimelineWidth));
                OnPropertyChanged(nameof(TimelineStart));
                OnPropertyChanged(nameof(TodayX));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}