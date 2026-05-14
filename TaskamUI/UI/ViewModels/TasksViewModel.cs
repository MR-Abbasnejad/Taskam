using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Taskam.Data.Models;
using Taskam.Data.Services;
using TaskamUI.Helpers;

namespace TaskamUI.ViewModels
{
    public class TasksViewModel : INotifyPropertyChanged
    {
        private readonly TaskService _taskService = new TaskService();

        public ObservableCollection<TaskItem> Tasks { get; set; }
            = new ObservableCollection<TaskItem>();

        private string _newTaskTitle = "";
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set { _newTaskTitle = value; OnPropertyChanged(); }
        }

        // ------------------------------
        // FILTERING
        // ------------------------------
        public enum TaskFilter
        {
            Today,
            Completed,
            Uncompleted
        }

        private TaskFilter _currentFilter = TaskFilter.Today;

        // Filter Buttons
        public ICommand ShowTodayCommand { get; }
        public ICommand ShowCompletedCommand { get; }
        public ICommand ShowUncompletedCommand { get; }


        // Commands
        public ICommand AddTaskCommand { get; }
        public ICommand ToggleTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }


        public TasksViewModel()
        {
            // MAIN COMMANDS
            AddTaskCommand = new RelayCommand(async _ => await AddTaskAsync());
            ToggleTaskCommand = new RelayCommand(async p => await ToggleCompleteAsync((TaskItem)p));
            DeleteTaskCommand = new RelayCommand(async p => await DeleteTaskAsync((TaskItem)p));

            // FILTER COMMANDS
            ShowTodayCommand = new RelayCommand(async _ =>
            {
                _currentFilter = TaskFilter.Today;
                await LoadTasksAsync();
            });

            ShowCompletedCommand = new RelayCommand(async _ =>
            {
                _currentFilter = TaskFilter.Completed;
                await LoadTasksAsync();
            });

            ShowUncompletedCommand = new RelayCommand(async _ =>
            {
                _currentFilter = TaskFilter.Uncompleted;
                await LoadTasksAsync();
            });
        }


        // ------------------------------
        // LOAD + FILTER LOGIC
        // ------------------------------
        public async Task LoadTasksAsync()
        {
            Tasks.Clear();
            var all = await _taskService.GetAllTasksAsync();

            IEnumerable<TaskItem> filtered = all;

            switch (_currentFilter)
            {
                case TaskFilter.Today:
                    filtered = all.Where(t => t.CreatedAt.Date == DateTime.Today);
                    break;

                case TaskFilter.Completed:
                    filtered = all.Where(t => t.IsCompleted);
                    break;

                case TaskFilter.Uncompleted:
                    filtered = all.Where(t => !t.IsCompleted);
                    break;
            }

            foreach (var task in filtered)
                Tasks.Add(task);
        }


        // ------------------------------
        // CRUD Operations
        // ------------------------------
        private async Task AddTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
                return;

            await _taskService.AddTaskAsync(NewTaskTitle);
            NewTaskTitle = "";
            await LoadTasksAsync();
        }
        public async Task<bool> ToggleCompleteDirectAsync(TaskItem task)
        {
            if (task == null) return false;
            return await _taskService.ToggleCompleteAsync(task);
        }
        private async Task ToggleCompleteAsync(TaskItem task)
        {
            if (task == null) return;

            // Save the current state
            var originalState = task.IsCompleted;

            // Update database
            var success = await _taskService.ToggleCompleteAsync(task);

            if (!success)
            {
                // Revert on failure
                task.IsCompleted = originalState;
            }
            // Do NOT call LoadTasksAsync() - it causes flickering and performance issues
        }

        private async Task DeleteTaskAsync(TaskItem task)
        {
            if (task == null) return;

            await _taskService.DeleteTaskAsync(task);
            await LoadTasksAsync();
        }


        // Notify Property Changed
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
