using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Users.Commands;
using TaskManager.Application.Features.Users.Queries;

namespace TaskManager.Presentation.Api.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly ILogger<UsersController> logger;
        private readonly IMapper mapper;

        public UsersController(
            ILogger<UsersController> logger,
            IMapper mapper
            )
        {
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var command = mapper.Map<LoginCommand>(login);
            command.IpAddress = GetIPAddress();
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUser user)
        {
            var command = mapper.Map<CreateUserCommand>(user);
            command.IpAddress = GetIPAddress();
            return Ok(await Mediator.Send(command));
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUser user)
        {
            var command = mapper.Map<UpdateUserCommand>(user);
            return Ok(await Mediator.Send(command));
        }

        [Authorize]
        [HttpDelete("{userId}/delete")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid userId)
        {

            return Ok(await Mediator.Send(new DeleteUserCommand { UserId = userId.ToString() }));
        }

        [Authorize]
        [HttpGet("{userId}/get")]
        public async Task<IActionResult> GetUser([FromRoute] Guid userId)
        {

            return Ok(await Mediator.Send(new GetUserQuery { Id = userId.ToString() }));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
        {

            return Ok(await Mediator.Send(query));
        }

    }
}
