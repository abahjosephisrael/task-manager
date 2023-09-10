using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Specifications
{
    public class TaskSpecification : BaseSpecification<Domain.Entities.Task>
    {
        public TaskSpecification(
            Guid? projectId,
            Status? status,
            Priority? priority,
            string searchText,
            int? skip,
            int? take,
            bool? descending = false
            ) : base(x =>
            (!projectId.HasValue || x.ProjectId == projectId.Value) &&
            (!status.HasValue || x.Status == status.Value) &&
            (!priority.HasValue || x.Priority == priority.Value) &&
            (!x.Deleted) &&
            (string.IsNullOrWhiteSpace(searchText) ||
            (
                x.Description + " " +
                x.Title
            ).ToLower().Contains(searchText.ToLower()))
            )
        {
            if (descending.HasValue && descending.Value)
                ApplyOrderByDescending(x => x.Created);
            else
                ApplyOrderBy(x => x.Created);

            if (take.HasValue && skip.HasValue)
            {
                ApplyPaging(skip.Value, take.Value);
            }
        }
    }
}
