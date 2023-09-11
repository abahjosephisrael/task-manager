using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> Create(CreateNotification notification);
        Task<Notification> MarkAsRead(Guid id);
        Task<Guid> Delete(Guid id);
        Task<Notification> Get(Guid id);
        Task<(IReadOnlyList<Notification> records, int total)> GetAll(GetNotifications request);

    }
}
