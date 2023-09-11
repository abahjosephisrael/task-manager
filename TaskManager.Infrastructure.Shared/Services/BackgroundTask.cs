using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Shared.Services
{
    public class BackgroundTask : IBackgroundTask
    {
        private readonly IRepositoryAsync<Domain.Entities.Task> taskRepo;
        private readonly IRepositoryAsync<Domain.Entities.UserTask> userTaskRepo;
        private readonly INotificationService notificationService;

        public BackgroundTask(
            IRepositoryAsync<Domain.Entities.Task> taskRepo,
            IRepositoryAsync<Domain.Entities.UserTask> userTaskRepo,
            INotificationService notificationService
            )
        {
            this.taskRepo = taskRepo;
            this.userTaskRepo = userTaskRepo;
            this.notificationService = notificationService;
        }
        public async Task SendDueNotification()
        {
            var tasks = await taskRepo.GetAllAsync();
            tasks = tasks.Where(x=> Math.Abs(x.DueDate.Subtract(DateTime.UtcNow).TotalHours) <= 48).ToList();
            foreach (var task in tasks)
            {
                var userTask = await userTaskRepo.GetByAsync(x => x.TaskId == task.Id);
                if (userTask != null)
                {
                    BackgroundJob.Enqueue(
                    () =>
                    System.Threading.Tasks.Task.WhenAll(
                    notificationService.Create(new CreateNotification
                    {
                        Message = $"Task Due: '{task.Title}' assigned to you, will be due within the next 48 hours",
                        Type = NotificationType.Update,
                        UserId = Guid.Parse(userTask.UserId),
                    }))
                    // You can add email service here
                    );
                }
            }
        }
    }
}
