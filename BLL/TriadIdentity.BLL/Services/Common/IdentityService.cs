using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TriadIdentity.BLL.Constants;
using TriadIdentity.BLL.Interfaces.Common;
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

        public IdentityService(
            IUnitOfWork unitOfWork,
            CustomUserManager userManager,
            CustomRoleManager roleManager,
            SignInManager<User> signInManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        public async Task RegisterUserAsync(RegisterUserDto dto)
        {
            ValidateDto(dto);

            var user = _mapper.Map<User>(dto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    throw new Exception(ErrorMessages.UserRegistrationFailed);
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
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

        private void ValidateDto<T>(T dto)
        {
            var context = new ValidationContext(dto);
            Validator.ValidateObject(dto, context, validateAllProperties: true);
        }
    }
}
