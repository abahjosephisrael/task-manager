using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Parameters;
using TaskManager.Application.Wrappers;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Features.Notifications.Queries
{
    public class GetNotificationsQuery : RequestParameter, IRequest<PagedResponse<List<NotificationResponse>>>
    {
        public NotificationType? Type { get; set; }
    }

    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PagedResponse<List<NotificationResponse>>>
    {
        private readonly INotificationService notificationService;
        private readonly IAuthenticatedUserService authenticatedUserService;

        public GetNotificationsQueryHandler(
            INotificationService notificationService,
            IAuthenticatedUserService authenticatedUserService
            )
        {
            this.notificationService = notificationService;
            this.authenticatedUserService = authenticatedUserService;
        }
        public async Task<PagedResponse<List<NotificationResponse>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var res = await notificationService.GetAll(new GetNotifications
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Type = request.Type,
                UserId = Guid.Parse(authenticatedUserService.UserId)
            });
            var processedRes = res.records.Select(x => new NotificationResponse
            {
                Message = x.Message,
                Id = x.Id,
                Read = x.Read,
                Type = x.Type.ToString()
            }).ToList();
            return new PagedResponse<List<NotificationResponse>>(processedRes, request.PageNumber, request.PageSize, res.total) { Message = $"{res.total} record(s) found" };
        }
    }
}
