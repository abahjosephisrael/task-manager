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

namespace TaskManager.Application.Features.Projects.Commands
{
    public class DeleteProjectCommandValidator: AbstractValidator<DeleteProjectCommand>
    {
        public DeleteProjectCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
    public class DeleteProjectCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }
    }
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Response<Guid>>
    {
        private readonly IRepositoryAsync<Project> projectRepo;

        public DeleteProjectCommandHandler(
            IRepositoryAsync<Project> projectRepo
            )
        {
            this.projectRepo = projectRepo;
        }
        public async Task<Response<Guid>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await projectRepo.GetByIdAsync(request.Id);
            if (project == null || project.Deleted) throw new KeyNotFoundException($"Project with ID:{request.Id} not found");
            project.Deleted = true;
            await projectRepo.UpdateAsync(project);
            return new Response<Guid>(project.Id,"Project deleted successfully");
        }
    }
}
