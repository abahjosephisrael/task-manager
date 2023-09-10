using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Wrappers;

namespace TaskManager.Application.Features.Users.Queries
{
    public class GetUserQuery : IRequest<Response<UserResponse>>
    {
        public string? Id { get; set; }
    }
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Response<UserResponse>>
    {
        private readonly IUserServices userServices;
        private readonly IAuthenticatedUserService authenticatedUserService;

        public GetUserQueryHandler(
            IUserServices userServices,
            IAuthenticatedUserService authenticatedUserService
            )
        {
            this.userServices = userServices;
            this.authenticatedUserService = authenticatedUserService;
        }
        public async Task<Response<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var user = await userServices.GetUser(request.Id ?? authenticatedUserService.UserId);
            return new Response<UserResponse>(user, "User found");
        }
    }
}
