using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Features.Projects.Commands;
using TaskManager.Application.Features.Projects.Queries;
using TaskManager.Application.Features.Tasks.Commands;
using TaskManager.Application.Features.Tasks.Queries;

namespace TaskManager.Presentation.Api.Controllers
{
    [Authorize]
    public class TasksController : BaseApiController 
    {

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
        
        [HttpPut("{taskId}/mark-completed")]
        public async Task<IActionResult> MarkAsCompleted([FromRoute] Guid taskId)
        {
            return Ok(await Mediator.Send(new UpdateTaskCommand
            {
                Id = taskId,
                Status = Domain.Enums.Status.Completed
            }));
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteTask([FromRoute] DeleteTaskCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetTaskById([FromRoute] GetTaskByIdQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetTasks([FromQuery] GetTasksQuery query)
        {
            return Ok(await Mediator.Send(query));
        }
    }
}
