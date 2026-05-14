using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Taskam.Data.Models;
using Taskam.Data.Services;

namespace TaskamUI.UI.ViewModels
{
    public class KanbanWindowViewModel : INotifyPropertyChanged
    {
        private readonly KanbanService _kanbanService = new();
        private readonly int _projectId;

        public ObservableCollection<Backlog> TodoItems { get; } = new();
        public ObservableCollection<Backlog> InProgressItems { get; } = new();
        public ObservableCollection<Backlog> DoneItems { get; } = new();

        private string _newBacklogTitle = "";
        public string NewBacklogTitle
        {
            get => _newBacklogTitle;
            set
            {
                if (_newBacklogTitle != value)
                {
                    _newBacklogTitle = value;
                    OnPropertyChanged(nameof(NewBacklogTitle));
                    // Refresh CanExecute state of AddBacklogCommand
                    (AddBacklogCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand AddBacklogCommand { get; }
        public ICommand DeleteBacklogCommand { get; }
        public ICommand MoveToInProgressCommand { get; }
        public ICommand MoveToDoneCommand { get; }
        public ICommand MoveBackToTodoCommand { get; }

        public KanbanWindowViewModel(int projectId)
        {
            _projectId = projectId;

            AddBacklogCommand = new RelayCommand(async () => await AddBacklog(),
                () => !string.IsNullOrWhiteSpace(NewBacklogTitle));
            DeleteBacklogCommand = new RelayCommand<Backlog>(async b => await DeleteBacklog(b));
            MoveToInProgressCommand = new RelayCommand<Backlog>(async b => await MoveBacklog(b, "In Progress"));
            MoveToDoneCommand = new RelayCommand<Backlog>(async b => await MoveBacklog(b, "Done"));
            MoveBackToTodoCommand = new RelayCommand<Backlog>(async b => await MoveBacklog(b, "Todo"));

            // Fire-and-forget with safe exception handling
            _ = LoadBacklogsAsync();
        }

        private async Task LoadBacklogsAsync()
        {
            try
            {
                var all = await _kanbanService.GetBacklogsByProjectAsync(_projectId);
                TodoItems.Clear();
                InProgressItems.Clear();
                DoneItems.Clear();

                foreach (var item in all.OrderBy(b => b.CreatedAt))
                {
                    switch (item.Status)
                    {
                        case "Todo": TodoItems.Add(item); break;
                        case "In Progress": InProgressItems.Add(item); break;
                        case "Done": DoneItems.Add(item); break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load backlogs: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddBacklog()
        {
            try
            {
                var newItem = await _kanbanService.CreateBacklogAsync(NewBacklogTitle, _projectId);
                if (newItem != null)
                {
                    TodoItems.Add(newItem);
                    NewBacklogTitle = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add backlog: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteBacklog(Backlog backlog)
        {
            if (backlog == null) return;
            if (MessageBox.Show($"Delete '{backlog.Name}'?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                await _kanbanService.DeleteBacklogAsync(backlog.Id);
                RemoveFromCollections(backlog);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete backlog: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task MoveBacklog(Backlog backlog, string newStatus)
        {
            if (backlog == null || backlog.Status == newStatus) return;

            try
            {
                var updated = await _kanbanService.UpdateBacklogStatusAsync(backlog.Id, newStatus);
                if (updated != null)
                {
                    RemoveFromCollections(backlog);
                    backlog.Status = newStatus;
                    AddToCollection(backlog);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to move backlog: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveFromCollections(Backlog backlog)
        {
            TodoItems.Remove(backlog);
            InProgressItems.Remove(backlog);
            DoneItems.Remove(backlog);
        }

        private void AddToCollection(Backlog backlog)
        {
            switch (backlog.Status)
            {
                case "Todo": TodoItems.Add(backlog); break;
                case "In Progress": InProgressItems.Add(backlog); break;
                case "Done": DoneItems.Add(backlog); break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // RelayCommand with CanExecuteChanged support
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (() => true);
        }

        public bool CanExecute(object? parameter) => _canExecute();
        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (t => true);
        }

        public bool CanExecute(object? parameter) => _canExecute((T)parameter!);
        public void Execute(object? parameter) => _execute((T)parameter!);

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}