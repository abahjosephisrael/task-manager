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
    public class UpdateUserCommandValidation: AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidation() 
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
        }
    }
    public class UpdateUserCommand : UpdateUser, IRequest<Response<string>>
    {
    }
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Response<string>>
    {
        private readonly IUserServices userServices;

        public UpdateUserCommandHandler(
            IUserServices userServices
            )
        {
            this.userServices = userServices;
        }
        public async Task<Response<string>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var response = await userServices.UpdateUser(request);
            return new Response<string>(response, "User updated successfully");
        }
    }
}
