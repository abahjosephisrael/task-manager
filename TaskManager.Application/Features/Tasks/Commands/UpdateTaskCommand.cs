using AutoMapper;
using FluentValidation;
using Hangfire;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Wrappers;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Features.Tasks.Commands
{
    public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
    {
        public UpdateTaskCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
    
    public class UpdateTaskCommand : IRequest<Response<TaskResponse>>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority? Priority { get; set; }
        public Status? Status { get; set; }
        public Guid Id { get; set; }
    }

    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Response<TaskResponse>>
    {
        private readonly IRepositoryAsync<Domain.Entities.Task> taskRepo;
        private readonly IRepositoryAsync<UserTask> userTaskRepo;
        private readonly INotificationService notificationService;
        private readonly IMapper mapper;

        public UpdateTaskCommandHandler(
            IRepositoryAsync<Domain.Entities.Task> taskRepo,
            IRepositoryAsync<Domain.Entities.UserTask> userTaskRepo,
            INotificationService notificationService,
            IMapper mapper
            )
        {
            this.taskRepo = taskRepo;
            this.userTaskRepo = userTaskRepo;
            this.notificationService = notificationService;
            this.mapper = mapper;
        }
        public async Task<Response<TaskResponse>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await taskRepo.GetByIdAsync(request.Id);
            if (task == null) throw new KeyNotFoundException($"Task with ID:{request.Id} not found");

            task.Priority = request.Priority ?? task.Priority;
            task.Status = request.Status ?? task.Status;
            task.DueDate = request.DueDate ?? task.DueDate;
            task.Description = request.Description ?? task.Description;
            task.Title = request.Title ?? task.Title;

            await taskRepo.UpdateAsync(task);
            var res = new TaskResponse
            {
                Id = task.Id,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority.ToString(),
                Status = task.Status.ToString(),
                Title = task.Title,
            };
            if(request.Status.HasValue && request.Status.Value == Status.Completed)
            {
                var userTask = await userTaskRepo.GetByAsync(x => x.TaskId == task.Id);
                if (userTask != null)
                {
                    BackgroundJob.Enqueue(
                    () =>
                    System.Threading.Tasks.Task.WhenAll(
                    notificationService.Create(new CreateNotification
                    {
                        Message = $"Task Completed: '{task.Title}' assigned to you, has been completed",
                        Type = NotificationType.Update,
                        UserId = Guid.Parse(userTask.UserId),
                    }))
                    // You can add email service here
                    );
                }
            }
            return new Response<TaskResponse>(res, "Task updated successfully");
        }
    }
}
