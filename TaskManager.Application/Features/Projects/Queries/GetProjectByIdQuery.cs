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

namespace TaskManager.Application.Features.Projects.Queries
{
    public class GetProjectByIdQuery : IRequest<Response<ProjectResponse>>
    {
        public Guid Id { get; set; }
    }

    public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Response<ProjectResponse>>
    {
        private readonly IRepositoryAsync<Project> projectRepo;

        public GetProjectByIdQueryHandler(
            IRepositoryAsync<Project> projectRepo
            )
        {
            this.projectRepo = projectRepo;
        }
        public async Task<Response<ProjectResponse>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
        {
            var project = await projectRepo.GetByAsync(x=>x.Id==request.Id, x=>x.Tasks);
            if (project == null) throw new KeyNotFoundException($"Project with ID:{request.Id} not found");
            var projectResponse = new ProjectResponse
            {
                Description = project.Description,
                Id = project.Id,
                Name = project.Name,
                Tasks = project.Tasks.Select(x => new TaskResponse
                {
                    Id = x.Id,
                    Description = x.Description,
                    DueDate = x.DueDate,
                    Priority = x.Priority.ToString(),
                    Status = x.Status.ToString(),
                    Title = x.Title,
                }).ToList()
            };

            return new Response<ProjectResponse>(projectResponse, "Poject found");
        }
    }
}
