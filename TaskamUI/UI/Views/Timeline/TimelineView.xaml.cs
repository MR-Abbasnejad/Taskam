using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Taskam.Data.Models;
using TaskamUI.UI.ViewModels;
using TaskamUI.UI.Views.Kanban;

namespace TaskamUI.UI.Views.Timeline
{
    public partial class TimelineView : UserControl
    {
        private TimelineViewModel _viewModel = null!;

        public TimelineView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = (TimelineViewModel)DataContext;
            _viewModel.Projects.CollectionChanged += Projects_CollectionChanged;
            RenderBars();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.Projects.CollectionChanged -= Projects_CollectionChanged;
        }

        private void Projects_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RenderBars();
        }

        private async void AddProjectClick(object sender, RoutedEventArgs e)
        {
            var window = new AddProjectWindow { Owner = Window.GetWindow(this) };
            if (window.ShowDialog() == true)
            {
                await _viewModel.AddProjectAsync(window.ProjectName, window.StartDate, window.EndDate);
                RenderBars();
            }
        }

        private async void ProjectName_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is KanbanProject project)
            {
                var kanbanWindow = new KanbanWindow(project.Id) { Owner = Window.GetWindow(this) };
                kanbanWindow.ShowDialog();
                await ReloadTimeline();
            }
        }

        private async void DeleteProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu ctxMenu &&
                ctxMenu.PlacementTarget is Button btn && btn.Tag is KanbanProject project)
            {
                await _viewModel.DeleteProjectAsync(project);
                RenderBars();
            }
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            // Find the parent ScrollViewer
            var scrollViewer = FindParent<ScrollViewer>(this);
            double targetX = _viewModel.TodayX;
            if (scrollViewer != null && targetX > 0)
            {
                scrollViewer.ScrollToHorizontalOffset(targetX - scrollViewer.ViewportWidth / 2);
            }
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && parent is not T)
                parent = VisualTreeHelper.GetParent(parent);
            return parent as T;
        }

        private async Task ReloadTimeline()
        {
            await _viewModel.LoadProjectsAsync();
            RenderBars();
        }

        private void RenderBars()
        {
            if (_viewModel == null || _viewModel.Projects.Count == 0)
            {
                BarsCanvas.Children.Clear();
                return;
            }

            BarsCanvas.Children.Clear();

            for (int i = 0; i < _viewModel.Projects.Count; i++)
            {
                var proj = _viewModel.Projects[i];
                double left = _viewModel.GetLeftOffset(proj.StartDate);
                double width = _viewModel.GetWidth(proj.StartDate, proj.EndDate);
                double top = _viewModel.GetTopOffset(i);

                var bar = new Border
                {
                    Style = (Style)Resources["TimelineBarStyle"],
                    Background = GetBrushForProject(i),
                    Width = width,
                    ToolTip = $"{proj.Title} | {proj.StartDate:dd MMM yyyy} - {proj.EndDate:dd MMM yyyy}"
                };
                Canvas.SetLeft(bar, left);
                Canvas.SetTop(bar, top);
                BarsCanvas.Children.Add(bar);
            }
        }

        private Brush GetBrushForProject(int index)
        {
            Brush[] colors =
            {
                new SolidColorBrush(Color.FromRgb(79, 163, 255)),
                new SolidColorBrush(Color.FromRgb(139, 233, 127)),
                new SolidColorBrush(Color.FromRgb(255, 121, 198)),
                new SolidColorBrush(Color.FromRgb(241, 196, 15))
            };
            return colors[index % colors.Length];
        }
    }
}