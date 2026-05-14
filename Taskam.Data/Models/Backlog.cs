using System;

namespace Taskam.Data.Models
{
    public class Backlog
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Todo"; // Todo, InProgress, Suspended, Done

        public int? KanbanProjectId { get; set; }

        public KanbanProject? KanbanProject { get; set; }
    }
}