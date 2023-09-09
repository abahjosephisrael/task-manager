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

        public async Task<string> CreateUser(CreateUser request)
        {
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var user = new User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserName = request.Email
                };
                var result = await _userManager.CreateAsync(user, request.Password.Trim());
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    return user.Id;
                }
                else
                {
                    throw new ApiException($"{result.Errors.FirstOrDefault().Description}");
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

        private string RandomTokenString()
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
            var account = await _userManager.FindByIdAsync(authenticatedUserService.UserId);
            if (account == null) throw new ApiException("Invalid account");
            account.FirstName = request.FirstName ?? account.FirstName;
            account.LastName = request.LastName ?? account.LastName;
            var result = await _userManager.UpdateAsync(account);
            if (!result.Succeeded) throw new ApiException(result.Errors.First().Description);
            return account.Id;
        }

        public async Task<RefreshTokenDto.Response> RefreshToken(RefreshTokenDto.Request request)
        {

            var refreshToken = _refreshTokenRepo.GetAllQuery().FirstOrDefault(r => r.Token == request.Token);
            if (refreshToken == null) throw new ApiException("Invalid token");
            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            if (user == null) throw new ApiException("Invalid token");

            if (!refreshToken.IsActive) throw new UnauthorizedAccessException("Token expired");


            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
            RefreshTokenDto.Response response = new RefreshTokenDto.Response();
            response.JwToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;

            var newRefreshToken = GenerateRefreshToken(request.IpAddress);
            newRefreshToken.UserId = user.Id;
            await _refreshTokenRepo.AddAsync(newRefreshToken);

            refreshToken.ReplacedByToken = newRefreshToken.Token;
            refreshToken.RevokedByIp = request.IpAddress;
            refreshToken.Revoked = DateTime.UtcNow;
            await _refreshTokenRepo.UpdateAsync(refreshToken);

            response.RefreshToken = newRefreshToken.Token;
            response.FirstName = user.FirstName;
            response.LastName = user.LastName;
            response.PhoneNumber = user.PhoneNumber;
            response.IsPhoneNumberVerified = user.PhoneNumberConfirmed;
            return response;
        }

        public async Task<UserResponse> GetUser(string accountId)
        {
            var account = await _userManager.FindByIdAsync(accountId);
            if (account == null) throw new KeyNotFoundException("Not.Found");
            var role = await _userManager.GetRolesAsync(account);
            return new UserResponse
            {
                Email = account.Email,
                FirstName = account.FirstName,
                Id = account.Id,
                LastName = account.LastName,
                LastLogin = account.LastLogin,
                Role = role.First()
            };
        }

        public (int total, List<UserResponse> data) GetUsers(int pageNumber, int pageSize)
        {
            var accounts = _userManager.Users.ToList();
            int total = accounts.Count;
            var accountsResponse = accounts
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
            return (total, accountsResponse);
        }

        protected async Task<Login.Response> GenerateLoginResponse(User user, string ipAddress, bool? isKey = false)
        {

            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
            var response = new Login.Response();
            response.Id = user.Id.ToString();
            response.JwToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();

            var refreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.ApplicationUserId = user.Id;
            await _refreshTokenRepo.AddAsync(refreshToken);

            response.RefreshToken = refreshToken.Token;
            response.FullName = user.FirstName + " " + user.LastName;
            response.LastName = user.LastName;
            response.FirstName = user.FirstName;
            response.HasPasscode = user.HasPasscode;
            response.PhoneNumber = user.PhoneNumber;
            response.PhoneCode = user.PhoneCode;
            response.IsPhoneNumberVerified = user.PhoneNumberConfirmed;
            response.IsOwner = user.IsOwner;
            response.TenantId = user.TenantId;
            response.IsProfileUpdated = user.IsProfileUpdated;

            var tenant = await tenantRepo.GetByAsync(x => x.Id == user.TenantId);

            response.LastLogin = user.LastLogin;
            response.TenantIdentifier = tenant?.Identifier;
            response.DOB = user.DOB;
            response.Environment = user.Environment;
            if (isKey.HasValue && !isKey.Value)
            {
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }
            return response;
        }

    }
}
