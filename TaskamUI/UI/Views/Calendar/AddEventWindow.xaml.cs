using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Taskam.Data.Models;

namespace TaskamUI.UI.Views.Calendar
{
    public partial class AddEventWindow : Window
    {
        public CalendarEvent? CreatedEvent { get; private set; }

        public AddEventWindow()
        {
            InitializeComponent();

            DayBox.ItemsSource = new[]
            {
                "Saturday",
                "Sunday",
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday"
            };

            StartHourBox.ItemsSource = Enumerable.Range(0, 24).ToList();
            DurationBox.ItemsSource = Enumerable.Range(1, 12).ToList();

            DayBox.SelectedIndex = 0;
            StartHourBox.SelectedIndex = 8;
            DurationBox.SelectedIndex = 0;
        }

        private void TitleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Fix: Check if PlaceholderText exists before using it
            if (PlaceholderText != null && TitleBox != null)
            {
                PlaceholderText.Visibility =
                    string.IsNullOrWhiteSpace(TitleBox.Text)
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Please enter a title.", "Missing Title", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartHourBox.SelectedItem == null || DurationBox.SelectedItem == null)
            {
                MessageBox.Show("Please select time and duration.", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Fix: Add color options
            CreatedEvent = new CalendarEvent
            {
                Title = TitleBox.Text.Trim(),
                Day = DayBox.SelectedIndex,
                StartHour = (int)StartHourBox.SelectedItem,
                DurationHours = (int)DurationBox.SelectedItem,
                Color = GetColorForDay(DayBox.SelectedIndex) // Set color based on day
            };

            DialogResult = true;
            Close();
        }

        private string GetColorForDay(int dayIndex)
        {
            // Different colors for different days
            return dayIndex switch
            {
                0 => "#4CAF50", // Saturday - Green
                1 => "#FF9800", // Sunday - Orange
                2 => "#2196F3", // Monday - Blue
                3 => "#9C27B0", // Tuesday - Purple
                4 => "#FF5722", // Wednesday - Deep Orange
                5 => "#E91E63", // Thursday - Pink
                6 => "#00BCD4", // Friday - Cyan
                _ => "#1E88E5"  // Default Blue
            };
        }
    }
}