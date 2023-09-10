using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Parameters;
using TaskManager.Application.Wrappers;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Features.Projects.Queries
{
    public class GetProjectsQuery : RequestParameter, IRequest<PagedResponse<List<ProjectResponse>>>
    {
    }

    public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PagedResponse<List<ProjectResponse>>>
    {
        private readonly IRepositoryAsync<Project> projectRepo;

        public GetProjectsQueryHandler(
            IRepositoryAsync<Project> projectRepo
            )
        {
            this.projectRepo = projectRepo;
        }
        public async Task<PagedResponse<List<ProjectResponse>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
        {
            var projects = await projectRepo.GetAllAsync();
            int total = projects.Count;
            var response = projects.Select(x => new ProjectResponse
            {
                Description = x.Description,
                Id = x.Id,
                Name = x.Name
            }).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();
            return new PagedResponse<List<ProjectResponse>>(response, request.PageNumber, request.PageSize, total) { Message = $"{total} record(s) found" };
        }
    }
}
