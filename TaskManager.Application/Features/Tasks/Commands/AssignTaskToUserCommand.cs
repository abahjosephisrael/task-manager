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
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Wrappers;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Features.Tasks.Commands
{
    public class AssignTaskToUserCommandValidator : AbstractValidator<AssignTaskToUserCommand>
    {
        public AssignTaskToUserCommandValidator()
        {
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
    
    public class AssignTaskToUserCommand : IRequest<Response<Guid>>
    {
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
    }

    public class AssignTaskToUserCommandHandler : IRequestHandler<AssignTaskToUserCommand, Response<Guid>>
    {
        private readonly IRepositoryAsync<Domain.Entities.Task> taskRepo;
        private readonly IRepositoryAsync<UserTask> userTaskRepo;
        private readonly IRepositoryAsync<User> userRepo;
        private readonly INotificationService notificationService;

        public AssignTaskToUserCommandHandler(
            IRepositoryAsync<Domain.Entities.Task> taskRepo,
            IRepositoryAsync<Domain.Entities.UserTask> userTaskRepo,
            IRepositoryAsync<User> userRepo,
            INotificationService notificationService
            )
        {
            this.taskRepo = taskRepo;
            this.userTaskRepo = userTaskRepo;
            this.userRepo = userRepo;
            this.notificationService = notificationService;
        }
        public async Task<Response<Guid>> Handle(AssignTaskToUserCommand request, CancellationToken cancellationToken)
        {
            var task = await taskRepo.GetByIdAsync(request.TaskId);
            if (task == null || task.Deleted) throw new KeyNotFoundException($"Task with ID:{request.TaskId} not found");

            var user = await userRepo.GetByAsync(x => x.Id == request.UserId.ToString());
            if (user == null) throw new KeyNotFoundException($"User with ID:{request.UserId} not found");

            if (userTaskRepo.GetAllQuery().Any(x => x.UserId == request.UserId.ToString() && x.TaskId == request.TaskId))
                throw new ApiException("Task Already Assigned To User");
            await userTaskRepo.AddAsync(new UserTask { Task = task, User = user, UserId = user.Id, TaskId = task.Id });

            BackgroundJob.Enqueue(
                    () =>
                    notificationService.Create(new CreateNotification
                    {
                        Message = $"Task Assignment: {task.Title} has been assigned to you. Due date is {task.DueDate.ToShortDateString()}",
                        Type = NotificationType.Assignment,
                        UserId = request.UserId
                    })
                    );

            return new Response<Guid>(task.Id, $"Task {task.Title} Assigned {user.FirstName} {user.LastName} successfully");
        }
    }
}
