using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Features.Notifications.Commands;
using TaskManager.Application.Features.Notifications.Queries;
using TaskManager.Application.Features.Projects.Commands;
using TaskManager.Application.Features.Projects.Queries;

namespace TaskManager.Presentation.Api.Controllers
{
    [Authorize]
    public class NotificationsController : BaseApiController 
    {


        [HttpPut("{Id}/mark-as-read")]
        public async Task<IActionResult> MarkNotificationAsRead([FromRoute] MarkNotificationAsReadCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetNotifications([FromQuery] GetNotificationsQuery query)
        {
            return Ok(await Mediator.Send(query));
        }
    }
}
