using System;
using System.Windows;

namespace TaskamUI.UI.Views.Timeline
{
    public partial class AddProjectWindow : Window
    {
        public string ProjectName { get; private set; } = "";
        public DateTime StartDate { get; private set; } = DateTime.Today;
        public DateTime EndDate { get; private set; } = DateTime.Today.AddDays(7);

        public AddProjectWindow()
        {
            InitializeComponent();
        }

        private void ProjectNameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ProjectNamePlaceholder.Visibility = string.IsNullOrEmpty(ProjectNameBox.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void GetProjectInfo_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(ProjectNameBox.Text))
            {
                MessageBox.Show("Please enter a project name.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a start date.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select an end date.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProjectName = ProjectNameBox.Text.Trim();
            StartDate = StartDatePicker.SelectedDate.Value;
            EndDate = EndDatePicker.SelectedDate.Value;

            if (EndDate < StartDate)
            {
                MessageBox.Show("End date must be after start date.", "Invalid Dates", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}