using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Commons;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities
{
    public class Notification: AuditableBaseEntity
    {
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public Guid UserId { get; set; }
        public bool Read { get; set; }
    }
}
