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

namespace TaskManager.Application.Features.Users.Queries
{
    public class GetUsersQuery : RequestParameter, IRequest<PagedResponse<List<UserResponse>>>
    {
    }
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResponse<List<UserResponse>>>
    {
        private readonly IUserServices userServices;

        public GetUsersQueryHandler(
            IUserServices userServices
            )
        {
            this.userServices = userServices;
        }
        public async Task<PagedResponse<List<UserResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var usersRes = await userServices.GetUsers(request.PageNumber,request.PageSize);
            return new PagedResponse<List<UserResponse>>(usersRes.data, request.PageNumber, request.PageSize, usersRes.total) { Message = $"{usersRes.total} record(s) found" };
        }
    }
}
