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
    public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
    {
        public DeleteTaskCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
    
    public class DeleteTaskCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Response<Guid>>
    {
        private readonly IRepositoryAsync<Domain.Entities.Task> taskRepo;

        public DeleteTaskCommandHandler(
            IRepositoryAsync<Domain.Entities.Task> taskRepo
            )
        {
            this.taskRepo = taskRepo;
        }
        public async Task<Response<Guid>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await taskRepo.GetByIdAsync(request.Id);
            if (task == null || task.Deleted) throw new KeyNotFoundException($"Task with ID:{request.Id} not found");

            task.Deleted = true;

            await taskRepo.UpdateAsync(task);
            return new Response<Guid>(task.Id, "Task deleted successfully");
        }
    }
}
