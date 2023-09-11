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
    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
        }
    }

    public class CreateProjectCommand : IRequest<Response<ProjectResponse>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Response<ProjectResponse>>
    {
        private readonly IRepositoryAsync<Project> projectRepo;
        private readonly IMapper mapper;

        public CreateProjectCommandHandler(
            IRepositoryAsync<Project> projectRepo,
            IMapper mapper
            )
        {
            this.projectRepo = projectRepo;
            this.mapper = mapper;
        }
        public async Task<Response<ProjectResponse>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = mapper.Map<Project>(request);
            await projectRepo.AddAsync(project);
            var response = mapper.Map<ProjectResponse>(project);
            return new Response<ProjectResponse>(response,"Project created successfully");
        }
    }
}
