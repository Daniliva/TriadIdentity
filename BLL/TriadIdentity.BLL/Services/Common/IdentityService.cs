using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TriadIdentity.BLL.Constants;
using TriadIdentity.BLL.Interfaces.Common;
using TriadIdentity.BLL.Interfaces.Identity;
using TriadIdentity.BLL.Models.Identity;
using TriadIdentity.DAL.Entities.Common;
using TriadIdentity.DAL.Entities.Identity;
using TriadIdentity.DAL.Interfaces;

namespace TriadIdentity.BLL.Services.Common
{
    public class IdentityService : IIdentityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CustomUserManager _userManager;
        private readonly CustomRoleManager _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        private readonly ILogService _logService;

        public IdentityService(
            IUnitOfWork unitOfWork,
            CustomUserManager userManager,
            CustomRoleManager roleManager,
            SignInManager<User> signInManager,
            IMapper mapper,
            ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _logService = logService;
        }

        public async Task RegisterUserAsync(RegisterUserDto dto)
        {
            ValidateDto(dto);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = _mapper.Map<User>(dto);
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    throw new Exception(ErrorMessages.UserRegistrationFailed);
                }

                await _logService.LogActionAsync("UserRegistered", user.Id.ToString(), $"User {dto.Email} registered at {DateTime.UtcNow}.");
            });
        }

        public async Task<string> LoginUserAsync(LoginUserDto dto)
        {
            ValidateDto(dto);

            var user = await _unitOfWork.Repository<User>().FindAsync(u => u.Email == dto.Email).Result.FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception(ErrorMessages.InvalidCredentials);
            }

            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, dto.RememberMe, false);
            if (!result.Succeeded)
            {
                throw new Exception(ErrorMessages.InvalidCredentials);
            }

            await _unitOfWork.Repository<LogEntry>().AddAsync(new LogEntry
            {
                Action = "UserLoggedIn",
                UserId = user.Id.ToString(),
                Details = $"User {dto.Email} logged in at {DateTime.UtcNow}."
            });
            await _unitOfWork.SaveChangesAsync();

            return "Login successful";
        }

        public async Task UpdateUserAsync(UpdateUserDto dto)
        {
            ValidateDto(dto);

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(dto.Id);
            if (user == null)
            {
                throw new Exception(ErrorMessages.UserNotFound);
            }

            _mapper.Map(dto, user);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception(ErrorMessages.UserUpdateFailed);
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task AssignRoleAsync(Guid userId, string roleName)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception(ErrorMessages.UserNotFound);
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new Role { Name = roleName });
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (!result.Succeeded)
                {
                    throw new Exception(ErrorMessages.RoleAssignmentFailed);
                }

                await _unitOfWork.Repository<LogEntry>().AddAsync(new LogEntry
                {
                    Action = "RoleAssigned",
                    UserId = user.Id.ToString(),
                    Details = $"Role {roleName} assigned to user {user.Email} at {DateTime.UtcNow}."
                });

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception(ErrorMessages.UserNotFound);
            }

            var userDto = _mapper.Map<UserDto>(user);
            ValidateDto(userDto);
            await _logService.LogActionAsync("UserRetrieved", userId.ToString(), $"User data retrieved for ID {userId} at {DateTime.UtcNow}.");
            return userDto;
        }

        public async Task<UserDto> GetUserByIdAsync(string email)
        {
            var user = await _unitOfWork.Repository<User>().FindAsync(u => u.Email == email).Result.FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception(ErrorMessages.UserNotFound);
            }

            var userDto = _mapper.Map<UserDto>(user);
            ValidateDto(userDto);
            await _logService.LogActionAsync("UserRetrieved", user.Id.ToString(), $"User data retrieved for email {email} at {DateTime.UtcNow}.");
            return userDto;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception(ErrorMessages.UserNotFound);
            }

            var roles = await _userManager.GetRolesAsync(user);
            await _logService.LogActionAsync("RolesRetrieved", userId.ToString(), $"Roles retrieved for user {user.Email} at {DateTime.UtcNow}.");
            return roles;
        }

        public async Task<IEnumerable<LogEntryDto>> GetUserLogsAsync(string userId)
        {
            var logs = await _logService.GetLogsByUserIdAsync(userId);
            await _logService.LogActionAsync("LogsRetrieved", userId, $"Logs retrieved for user at {DateTime.UtcNow}.");
            return logs;
        }

        private void ValidateDto<T>(T dto)
        {
            var context = new ValidationContext(dto);
            Validator.ValidateObject(dto, context, validateAllProperties: true);
        }
    }

}