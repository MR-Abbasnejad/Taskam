using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Taskam.Data.Data;
using Taskam.Data.Models;
using Taskam.Data.Services;
using TaskamUI.UI.ViewModels;

namespace TaskamUI.UI.Views.Calendar
{
    public partial class CalendarView : UserControl
    {
        private readonly CalendarViewModel _viewModel;
        private readonly SystemTimeService _timeService;
        private bool _isSyncing = false;

        private const double HourHeight = 60;

        public CalendarView()
        {
            InitializeComponent();

            var db = new TaskamDbContext();
            var timeService = new SystemTimeService();
            var calendarService = new CalendarService(db);

            _viewModel = new CalendarViewModel(calendarService, timeService);
            _timeService = timeService;

            DataContext = _viewModel;

            Loaded += CalendarView_Loaded;
            SizeChanged += CalendarView_SizeChanged;

            _timeService.PropertyChanged += TimeService_PropertyChanged;
        }

        public CalendarView(CalendarViewModel viewModel, SystemTimeService timeService)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _timeService = timeService;

            DataContext = _viewModel;

            Loaded += CalendarView_Loaded;
            SizeChanged += CalendarView_SizeChanged;

            _timeService.PropertyChanged += TimeService_PropertyChanged;
        }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isSyncing) return;

            _isSyncing = true;

            if (TimeScrollViewer != null)
            {
                TimeScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
            }

            _isSyncing = false;
        }

        private void TimeScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isSyncing) return;

            _isSyncing = true;

            if (MainScrollViewer != null)
            {
                MainScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
            }

            _isSyncing = false;
        }

        private async void CalendarView_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadEventsAsync();
            RenderEvents();
        }

        private void CalendarView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderEvents();
        }

        private void TimeService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemTimeService.CurrentDateTime))
            {
                Dispatcher.Invoke(RenderEvents);
            }
        }

        private async void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEventWindow
            {
                Owner = Window.GetWindow(this)
            };

            if (window.ShowDialog() == true && window.CreatedEvent != null)
            {
                await _viewModel.AddEventAsync(window.CreatedEvent);
                RenderEvents();
            }
        }

        private void RenderEvents()
        {
            if (CalendarBody == null || CalendarBody.ActualWidth <= 0)
                return;

            if (_viewModel?.Events == null)
                return;

            if (EventCanvas == null)
                return;

            EventCanvas.Children.Clear();

            double dayWidth = CalendarBody.ActualWidth / 7.0;

            if (dayWidth <= 0) return;

            foreach (var ev in _viewModel.Events)
            {
                if (ev == null) continue;

                var card = CreateEventCard(ev, dayWidth);
                if (card == null) continue;

                Canvas.SetLeft(card, ev.Day * dayWidth + 4);
                Canvas.SetTop(card, ev.StartHour * HourHeight + 4);

                EventCanvas.Children.Add(card);
            }

            RenderCurrentTimeLine(dayWidth);
        }

        private void RenderCurrentTimeLine(double dayWidth)
        {
            if (dayWidth <= 0) return;

            int currentDay = ConvertDay(DateTime.Now.DayOfWeek);
            TimeSpan time = _timeService.CurrentTime;

            double top = (time.Hours * HourHeight) + ((time.Minutes / 60.0) * HourHeight);
            double left = currentDay * dayWidth;

            var line = new Border
            {
                Width = dayWidth,
                Height = 2,
                Background = Brushes.Red
            };

            Canvas.SetLeft(line, left);
            Canvas.SetTop(line, top);
            EventCanvas.Children.Add(line);

            var dot = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Red
            };

            Canvas.SetLeft(dot, left - 5);
            Canvas.SetTop(dot, top - 4);
            EventCanvas.Children.Add(dot);
        }

        private int ConvertDay(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Saturday => 0,
                DayOfWeek.Sunday => 1,
                DayOfWeek.Monday => 2,
                DayOfWeek.Tuesday => 3,
                DayOfWeek.Wednesday => 4,
                DayOfWeek.Thursday => 5,
                DayOfWeek.Friday => 6,
                _ => 0
            };
        }

        private Border? CreateEventCard(CalendarEvent ev, double dayWidth)
        {
            if (ev == null || dayWidth <= 0) return null;

            Brush background;
            try
            {
                background = (SolidColorBrush)new BrushConverter().ConvertFromString(ev.Color) ?? Brushes.DodgerBlue;
            }
            catch
            {
                background = Brushes.DodgerBlue;
            }

            var border = new Border
            {
                Width = dayWidth - 8,
                Height = ev.DurationHours * HourHeight - 8,
                Background = background,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8),
                Cursor = Cursors.Hand
            };

            var panel = new StackPanel();

            panel.Children.Add(new TextBlock
            {
                Text = ev.Title,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            });

            panel.Children.Add(new TextBlock
            {
                Text = $"{ev.StartHour:00}:00",
                Foreground = Brushes.WhiteSmoke,
                FontSize = 11,
                Margin = new Thickness(0, 4, 0, 0)
            });

            border.Child = panel;

            border.MouseLeftButtonDown += async (s, e) =>
            {
                var result = MessageBox.Show(
                    $"Delete '{ev.Title}' ?",
                    "Delete Event",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _viewModel.DeleteEventAsync(ev);
                    RenderEvents();
                }
            };

            return border;
        }
    }
}