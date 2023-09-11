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
    public class CreateUserCommandValidation: AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidation() 
        {
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.ConfirmPassword).Equal(x=>x.Password);
        }
    }
    public class CreateUserCommand : CreateUser, IRequest<Response<LoginResponse>>
    {
        public string IpAddress { get; set; }
    }
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<LoginResponse>>
    {
        private readonly IUserServices userServices;

        public CreateUserCommandHandler(
            IUserServices userServices
            )
        {
            this.userServices = userServices;
        }
        public async Task<Response<LoginResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var response = await userServices.CreateUser(request);
            return new Response<LoginResponse>(response, "User created successfully");
        }
    }
}
