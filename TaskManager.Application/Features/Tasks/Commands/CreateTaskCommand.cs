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
    public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
    {
        public CreateTaskCommandValidator()
        {
            RuleFor(x=>x.Title).NotEmpty();
            RuleFor(x=>x.Description).NotEmpty();
            RuleFor(x=>x.DueDate).NotEmpty();
            RuleFor(x=>x.ProjectId).NotEmpty();
            RuleFor(x=>x.Status).IsInEnum().NotEmpty();
            RuleFor(x=>x.Priority).IsInEnum().NotEmpty();
        }
    }
    
    public class CreateTaskCommand : IRequest<Response<TaskResponse>>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public Priority Priority { get; set; }
        public Status Status { get; set; }
        public Guid ProjectId { get; set; }
    }

    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Response<TaskResponse>>
    {
        private readonly IRepositoryAsync<Domain.Entities.Task> taskRepo;
        private readonly IRepositoryAsync<Project> projectRepo;
        private readonly IMapper mapper;

        public CreateTaskCommandHandler(
            IRepositoryAsync<Domain.Entities.Task> taskRepo,
            IRepositoryAsync<Project> projectRepo,
            IMapper mapper
            )
        {
            this.taskRepo = taskRepo;
            this.projectRepo = projectRepo;
            this.mapper = mapper;
        }
        public async Task<Response<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {

            var project = await projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null || project.Deleted) throw new KeyNotFoundException($"Project with ID:{request.ProjectId} not found");
            var task = mapper.Map<Domain.Entities.Task>(request);
            await taskRepo.AddAsync(task);
            var res = new TaskResponse
            {
                Id = task.Id,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority.ToString(),
                Status = task.Status.ToString(),
                Title = task.Title,
            };
            return new Response<TaskResponse>(res, "Task created successfully");
        }
    }
}
