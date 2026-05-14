using System;
using System.Threading.Tasks;
using System.Windows;
using Taskam.Data.Services;
using TaskamUI.UI.ViewModels;

namespace TaskamUI.UI.Views.Kanban
{
    public partial class KanbanWindow : Window
    {
        private readonly int _projectId;
        private readonly KanbanService _kanbanService = new();

        public KanbanWindow(int projectId)
        {
            InitializeComponent();
            _projectId = projectId;

            // Create the ViewModel with the project ID only
            this.DataContext = new KanbanWindowViewModel(_projectId);

            Loaded += async (s, e) => await LoadProjectData();
        }

        private async Task LoadProjectData()
        {
            var project = await _kanbanService.GetProjectByIdAsync(_projectId);
            if (project != null)
            {
                this.Title = $"Kanban - {project.Title}";
            }
        }
    }
}