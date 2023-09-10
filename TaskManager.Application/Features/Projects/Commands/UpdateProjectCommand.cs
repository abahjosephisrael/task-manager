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
    public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
        }
    }
    public class UpdateProjectCommand : IRequest<Response<ProjectResponse>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Response<ProjectResponse>>
    {
        private readonly IRepositoryAsync<Project> projectRepo;
        private readonly IMapper mapper;

        public UpdateProjectCommandHandler(
            IRepositoryAsync<Project> projectRepo,
            IMapper mapper
            )
        {
            this.projectRepo = projectRepo;
            this.mapper = mapper;
        }
        public async Task<Response<ProjectResponse>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await projectRepo.GetByIdAsync(request.Id);
            if (project == null || project.Deleted) throw new KeyNotFoundException($"Project with ID:{request.Id} not found");
            project.Name = request.Name??project.Name;
            project.Description = request.Description ?? project.Description;
            await projectRepo.UpdateAsync(project);
            var response = mapper.Map<ProjectResponse>(project);
            return new Response<ProjectResponse>(response,"Project updated successfully");
        }
    }
}
