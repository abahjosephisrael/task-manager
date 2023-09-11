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
    public class LoginCommandValidation: AbstractValidator<LoginCommand>
    {
        public LoginCommandValidation() 
        {
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
    public class LoginCommand : Login, IRequest<Response<LoginResponse>>
    {
        public string IpAddress { get; set; }
    }
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Response<LoginResponse>>
    {
        private readonly IUserServices userServices;

        public LoginCommandHandler(
            IUserServices userServices
            )
        {
            this.userServices = userServices;
        }
        public async Task<Response<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var response = await userServices.Login(request);
            return new Response<LoginResponse>(response, "Login Successful");
        }
    }
}
