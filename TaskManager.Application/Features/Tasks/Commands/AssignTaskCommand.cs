using AutoMapper;
using FluentValidation;
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
    public class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
    {
        public AssignTaskCommandValidator()
        {
            RuleFor(x => x.TaskId).NotEmpty();
            RuleFor(x => x.ProjectId).NotEmpty();
        }
    }
    
    public class AssignTaskCommand : IRequest<Response<Guid>>
    {
        public Guid TaskId { get; set; }
        public Guid ProjectId { get; set; }
    }

    public class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, Response<Guid>>
    {
        private readonly IRepositoryAsync<Domain.Entities.Task> taskRepo;
        private readonly IRepositoryAsync<Project> projectRepo;

        public AssignTaskCommandHandler(
            IRepositoryAsync<Domain.Entities.Task> taskRepo,
            IRepositoryAsync<Project> projectRepo
            )
        {
            this.taskRepo = taskRepo;
            this.projectRepo = projectRepo;
        }
        public async Task<Response<Guid>> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await taskRepo.GetByIdAsync(request.TaskId);
            if (task == null || task.Deleted) throw new KeyNotFoundException($"Task with ID:{request.TaskId} not found");

            var project = await projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null || project.Deleted) throw new KeyNotFoundException($"Project with ID:{request.ProjectId} not found");

            task.ProjectId = project.Id;

            await taskRepo.UpdateAsync(task);
            return new Response<Guid>(task.Id, $"Task Assigned successfully");
        }
    }
}
