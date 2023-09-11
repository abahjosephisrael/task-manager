using FluentValidation;
using FluentValidation.Validators;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Wrappers;

namespace TaskManager.Application.Features.Users.Commands
{
    public class DeleteUserCommand : IRequest<Response<string>>
    {
        public string UserId { get; set; }
    }
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Response<string>>
    {
        private readonly IUserServices userServices;

        public DeleteUserCommandHandler(
            IUserServices userServices
            )
        {
            this.userServices = userServices;
        }
        public async Task<Response<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var response = await userServices.DeleteUser(request.UserId);
            return new Response<string>(response, "User deleted successfully");
        }
    }
}
