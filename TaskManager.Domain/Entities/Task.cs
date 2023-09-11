using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Domain.Commons;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities
{
    public class Task : AuditableBaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public Priority Priority { get; set; }
        public Status Status { get; set; }
        public Project Project { get; set; }
        public Guid ProjectId { get; set; }

    }
}
