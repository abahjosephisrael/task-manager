using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Persistence.Helpers;
using TaskManager.Domain.Entities;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Settings;
using TaskManager.Application.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Users.Commands;

namespace TaskManager.Infrastructure.Persistence.Services
{
    public class UserServices : IUserServices
    {


        private readonly UserManager<Domain.Entities.User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<Domain.Entities.User> _signInManager;
        private readonly IRepositoryAsync<RefreshToken> _refreshTokenRepo;
        private readonly IAuthenticatedUserService authenticatedUserService;
        private readonly JWTSettings _jwtSettings;
        public UserServices(UserManager<Domain.Entities.User> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JWTSettings> jwtSettings,
            SignInManager<Domain.Entities.User> signInManager,
            IRepositoryAsync<RefreshToken> refreshTokenRepo,
            IAuthenticatedUserService authenticatedUserService
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _signInManager = signInManager;
            _refreshTokenRepo = refreshTokenRepo;
            this.authenticatedUserService = authenticatedUserService;
        }

        public async Task<LoginResponse> CreateUser(CreateUserCommand request)
        {
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var user = new User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserName = request.Email,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, request.Password.Trim());
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    var loginRes = await GenerateLoginResponse(user,request.IpAddress);
                    return loginRes;
                }
                else
                {
                    throw new ApiException($"{result?.Errors?.FirstOrDefault().Description}");
                }
            }
            else
            {
                throw new ApiException($"Email {request.Email} is already taken.");
            }
        }

        public async Task<bool> AddUserToRole(string roleId, string userId, bool removePreviousRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (removePreviousRoles)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Count > 0)
                    {
                        await _userManager.RemoveFromRolesAsync(user, roles);
                    }
                }
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null) throw new ApiException($"Invalid RoleId {roleId}");
                var result = await _userManager.AddToRoleAsync(user, role.Name);
                return result.Succeeded;
            }
            throw new ApiException($"Invalid.UserId");
        }

        private static string RandomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                Id = Guid.NewGuid(),
            };
        }

        private async Task<JwtSecurityToken> GenerateJWToken(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id.ToString()),
                new Claim("ip", IpHelper.GetIpAddress()),
                new Claim("uname",$"{user.FirstName} {user.LastName},{roles.FirstOrDefault()}")
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }

        public async Task<string> UpdateUser(UpdateUser request)
        {
            var user = await _userManager.FindByIdAsync(authenticatedUserService.UserId);
            if (user == null) throw new ApiException("Invalid user");
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) throw new ApiException(result.Errors.First().Description);
            return user.Id;
        }
        
        public async Task<string> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new ApiException("Invalid user");
            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) throw new ApiException(result.Errors.First().Description);
            return user.Id;
        }

        public async Task<UserResponse> GetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User Not Found");
            var role = await _userManager.GetRolesAsync(user);
            return new UserResponse
            {
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName,
                LastLogin = user.LastLogin,
                Role = role.First()
            };
        }

        public async Task<(int total, List<UserResponse> data)> GetUsers(int pageNumber, int pageSize)
        {
            var users = await _userManager.Users.ToListAsync();
            int total = users.Count;
            var usersResponse = users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new UserResponse
                {
                    Email = a.Email,
                    FirstName = a.FirstName,
                    Id = a.Id,
                    LastName = a.LastName,
                    LastLogin = a.LastLogin
                })
                .ToList();
            return (total, usersResponse);
        }


        public async Task<LoginResponse> Login(LoginCommand request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                throw new ApiException("There is no user with this user. If you are new, please Sign up");
            }
            if (!user.EmailConfirmed)
            {
                throw new ApiException("Account Not Confirmed");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password.Trim(), false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return await GenerateLoginResponse(user, request.IpAddress);
            }
            if (result.IsLockedOut)
            {
                throw new ApiException("Your user is locked due to many login trials. Try again after sometimes or reset your password.");
            }
            throw new ApiException("Incorrect email or password.\nYour user will be locked after 5 failed attempts.");
        }


        protected async Task<LoginResponse> GenerateLoginResponse(User user, string ipAddress)
        {

            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
            LoginResponse response = new()
            {
                Id = user.Id.ToString(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Email = user.Email
            };
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();

            var refreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.UserId = user.Id;
            await _refreshTokenRepo.AddAsync(refreshToken);

            response.FullName = user.FirstName + " " + user.LastName;
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            response.LastLogin = user.LastLogin.ToString();
            return response;
        }

    }
}
