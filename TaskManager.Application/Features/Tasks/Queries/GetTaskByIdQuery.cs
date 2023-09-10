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

namespace TaskManager.Application.Features.Tasks.Queries
{
    public class GetTaskByIdQueryValidator: AbstractValidator<GetTaskByIdQuery>
    {
        public GetTaskByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
    public class GetTaskByIdQuery : IRequest<Response<TaskResponse>>
    {
        public Guid Id { get; set; }
    }
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Response<TaskResponse>>
    {

        private readonly IRepositoryAsync<Domain.Entities.Task> taskRepo;

        public GetTaskByIdQueryHandler(
            IRepositoryAsync<Domain.Entities.Task> taskRepo
            )
        {
            this.taskRepo = taskRepo;
        }
        public async Task<Response<TaskResponse>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var task = await taskRepo.GetByIdAsync(request.Id);
            if (task == null) throw new KeyNotFoundException($"Task with ID:{request.Id} not found");

            var res = new TaskResponse
            {
                Id = task.Id,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority.ToString(),
                Status = task.Status.ToString(),
                Title = task.Title,
            };
            return new Response<TaskResponse>(res, "Task found");
        }
    }
}
