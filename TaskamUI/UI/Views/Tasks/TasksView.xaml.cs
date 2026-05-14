using System.Windows;
using System.Windows.Controls;
using Taskam.Data.Models;
using TaskamUI.ViewModels;

namespace TaskamUI.UI.Views.Tasks
{
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
            DataContext = new TasksViewModel();

            Loaded += TasksView_Loaded;
        }

        private async void TasksView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (TasksViewModel)DataContext;
            await vm.LoadTasksAsync();
        }

        private async void CheckBox_Toggled(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var task = checkbox?.DataContext as TaskItem;
            if (task != null)
            {
                var vm = (TasksViewModel)DataContext;

                // Store original state
                var originalState = task.IsCompleted;

                // Toggle UI immediately for responsiveness
                task.IsCompleted = !originalState;

                // Update database
                var success = await vm.ToggleCompleteDirectAsync(task);

                if (!success)
                {
                    // Revert on failure
                    task.IsCompleted = originalState;
                }
            }
        }
    }
}