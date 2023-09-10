using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Users.Commands;

namespace TaskManager.Application.Interfaces
{
    public interface IUserServices
    {
        Task<bool> AddUserToRole(string roleId, string userId, bool removePreviousRoles);
        Task<LoginResponse> CreateUser(CreateUserCommand request);
        Task<string> DeleteUser(string id);
        Task<UserResponse> GetUser(string accountId);
        Task<(int total, List<UserResponse> data)> GetUsers(int pageNumber, int pageSize);
        Task<LoginResponse> Login(LoginCommand request);
        Task<string> UpdateUser(UpdateUser request);
    }
}
