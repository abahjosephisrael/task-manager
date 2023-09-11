using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.Parameters;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs
{
    public class CreateNotification
    {
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
    }
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public bool Read { get; set; }
    }
    public class GetNotifications : RequestParameter
    {
        public Guid? UserId { get; set; }
        public NotificationType? Type { get; set; }

    }
}
