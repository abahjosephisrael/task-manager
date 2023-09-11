using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Wrappers;

namespace TaskManager.Application.Features.Notifications.Commands
{
    public class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
    {
        public MarkNotificationAsReadCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
    public class MarkNotificationAsReadCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }
    }

    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Response<Guid>>
    {
        private readonly INotificationService notificationService;

        public MarkNotificationAsReadCommandHandler(
            INotificationService notificationService
            )
        {
            this.notificationService = notificationService;
        }
        public async Task<Response<Guid>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var res = await notificationService.MarkAsRead(request.Id);
            return new Response<Guid>(res.Id, "Marked as read");
        }
    }
}
