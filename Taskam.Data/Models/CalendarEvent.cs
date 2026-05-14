using System.ComponentModel.DataAnnotations;

namespace Taskam.Data.Models
{
    public class CalendarEvent
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty; // Fixed: Added default value

        // 0 = Saturday
        // 6 = Friday
        public int Day { get; set; }

        public int StartHour { get; set; }

        public int DurationHours { get; set; }

        public string Color { get; set; } = "#FFFFFF"; // Fixed: Added default value
    }
}