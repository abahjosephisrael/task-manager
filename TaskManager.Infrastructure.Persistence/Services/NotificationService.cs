using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepositoryAsync<Notification> notificationRepo;
        private readonly IMapper mapper;

        public NotificationService(
            IRepositoryAsync<Notification> notificationRepo,
            IMapper mapper
            )
        {
            this.notificationRepo = notificationRepo;
            this.mapper = mapper;
        }
        public async Task<Notification> Create(CreateNotification notificationRequest)
        {
            var notification = mapper.Map<Notification>(notificationRequest);
            notification.Read = false;
            await notificationRepo.AddAsync(notification);
            return notification;
        }

        public async Task<Guid> Delete(Guid id)
        {
            var notification = await notificationRepo.GetByIdAsync(id);
            if (notification == null || notification.Deleted)
                throw new KeyNotFoundException($"Notification with ID:{id} not found");

            notification.Deleted = true;
            await notificationRepo.UpdateAsync(notification);
            return notification.Id;
        }

        public async Task<Notification> Get(Guid id)
        {
            var notification = await notificationRepo.GetByIdAsync(id);
            if (notification == null || notification.Deleted)
                throw new KeyNotFoundException($"Notification with ID:{id} not found");
            return notification;
        }

        public async Task<(IReadOnlyList<Notification> records,int total)> GetAll(GetNotifications request)
        {
            IReadOnlyList<Notification> notifications;
            int total = 0;
            if(request.UserId.HasValue && request.UserId.Value != Guid.Empty)
            {
                if (request.Type.HasValue)
                {
                    notifications = await notificationRepo.ListAsync(x => x.UserId == request.UserId.Value && x.Type == request.Type.Value); 
                    total = notifications.Count;
                    notifications = notifications
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();
                }
                else
                {
                    notifications = await notificationRepo.ListAsync(x => x.UserId == request.UserId.Value);
                    total = notifications.Count;
                    notifications = notifications
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();
                }
            }
            else
            {
                if (request.Type.HasValue)
                {
                    notifications = await notificationRepo.ListAsync(x =>x.Type == request.Type.Value);
                    total = notifications.Count;
                    notifications = notifications
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();
                }
                else
                {
                    notifications = await notificationRepo.GetPagedReponseAsync(request.PageNumber,request.PageSize);
                    total = notifications.Count;
                    notifications = notifications
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();
                }
            }
            return (notifications,total);
        }

        public async Task<Notification> MarkAsRead(Guid id)
        {
            var notification = await notificationRepo.GetByIdAsync(id);
            if (notification == null || notification.Deleted)
                throw new KeyNotFoundException($"Notification with ID:{id} not found");

            notification.Read = true;
            await notificationRepo.UpdateAsync(notification);
            return notification;
        }
    }
}
