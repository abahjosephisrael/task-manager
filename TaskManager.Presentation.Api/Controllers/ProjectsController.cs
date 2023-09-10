using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Features.Projects.Commands;
using TaskManager.Application.Features.Projects.Queries;

namespace TaskManager.Presentation.Api.Controllers
{
    [Authorize]
    public class ProjectsController : BaseApiController 
    {

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProject([FromBody] UpdateProjectCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteProject([FromRoute] DeleteProjectCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetProjectById([FromRoute] GetProjectByIdQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetProjects([FromQuery] GetProjectsQuery query)
        {
            return Ok(await Mediator.Send(query));
        }
    }
}
