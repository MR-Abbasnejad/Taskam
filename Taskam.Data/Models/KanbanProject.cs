using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskam.Data.Models
{
    public class KanbanProject
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Active"; // Active, Completed, Archived

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<Backlog> BacklogItems { get; set; } = new List<Backlog>();
    }
}