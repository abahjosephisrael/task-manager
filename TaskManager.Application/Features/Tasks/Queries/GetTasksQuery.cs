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
using TaskManager.Application.Parameters;
using TaskManager.Application.Specifications;
using TaskManager.Application.Wrappers;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Features.Tasks.Queries
{
    public class GetTasksQuery : RequestParameter, IRequest<PagedResponse<List<TaskResponse>>>
    {
        public Status? Status { get; set; }
        public Priority? Priority { get; set; }
        public string? SearchText { get; set; }
        public bool? Descending { get; set; }
        public Guid? ProjectId { get; set; }
    }
    public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PagedResponse<List<TaskResponse>>>
    {

        private readonly IRepositoryAsync<Domain.Entities.Task> taskRepo;

        public GetTasksQueryHandler(
            IRepositoryAsync<Domain.Entities.Task> taskRepo
            )
        {
            this.taskRepo = taskRepo;
        }
        public async Task<PagedResponse<List<TaskResponse>>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
        {
            var specification = new TaskSpecification(
                projectId: request.ProjectId,
                status: request.Status,
                priority: request.Priority,
                searchText: request.SearchText,
                skip: request.PageNumber,
                take: request.PageSize,
                descending: request.Descending
                );

            var countSpecification = new TaskSpecification(
                projectId: request.ProjectId,
                status: request.Status,
                priority: request.Priority,
                searchText: request.SearchText,
                skip: null,
                take: null,
                descending: request.Descending
                );
            var tasks = await taskRepo.ListAsync(specification);
            var total = await taskRepo.CountAsync(countSpecification);

            var res = tasks.Select(x=> new TaskResponse
            {
                Id = x.Id,
                Description = x.Description,
                DueDate = x.DueDate,
                Priority = x.Priority.ToString(),
                Status = x.Status.ToString(),
                Title = x.Title,
            }).ToList();
            return new PagedResponse<List<TaskResponse>>(res, request.PageNumber, request.PageSize, total) { Message = $"{total} record(s) found" };
        }
    }
}
