using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
                var validationErrors = ValidateDto(dto);
                if (validationErrors.Any())
                {
                    await _logService.LogActionAsync("ValidationError", null, $"Validation failed for RegisterUser: {string.Join("; ", validationErrors)}");
                    throw new ValidationException(string.Join("; ", validationErrors));
                }

                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    validationErrors.Add("Email is already taken.");
                    await _logService.LogActionAsync("ValidationError", null, $"Validation failed for RegisterUser: Email {dto.Email} is already taken.");
                    throw new ValidationException(string.Join("; ", validationErrors));
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var user = _mapper.Map<User>(dto);
                    var result = await _userManager.CreateAsync(user, dto.Password);
                    if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(e => e.Description).ToList();
                        await _logService.LogActionAsync("ValidationError", null, $"Identity validation failed: {string.Join("; ", errors)}");
                        throw new ValidationException(string.Join("; ", errors));
                    }

                    await _logService.LogActionAsync("UserRegistered", user.Id.ToString(), $"User {dto.Email} registered at {DateTime.UtcNow}.");
                });
            }

            public async Task<string> LoginUserAsync(LoginUserDto dto)
            {
                var validationErrors = ValidateDto(dto);
                if (validationErrors.Any())
                {
                    await _logService.LogActionAsync("ValidationError", null, $"Validation failed for LoginUser: {string.Join("; ", validationErrors)}");
                    throw new ValidationException(string.Join("; ", validationErrors));
                }

                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    await _logService.LogActionAsync("ValidationError", null, $"Invalid credentials for email {dto.Email}.");
                    throw new ValidationException(ErrorMessages.InvalidCredentials);
                }

                var result = await _signInManager.PasswordSignInAsync(user, dto.Password, dto.RememberMe, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    await _logService.LogActionAsync("ValidationError", null, $"Invalid credentials for email {dto.Email}.");
                    throw new ValidationException(ErrorMessages.InvalidCredentials);
                }

                await _logService.LogActionAsync("UserLoggedIn", user.Id.ToString(), $"User {dto.Email} logged in at {DateTime.UtcNow}.");
                return user.Id.ToString();
            }

            public async Task UpdateUserAsync(UpdateUserDto dto)
            {
                var validationErrors = ValidateDto(dto);
                if (validationErrors.Any())
                {
                    await _logService.LogActionAsync("ValidationError", null, $"Validation failed for UpdateUser: {string.Join("; ", validationErrors)}");
                    throw new ValidationException(string.Join("; ", validationErrors));
                }

                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null && existingUser.Id != dto.Id)
                {
                    validationErrors.Add("Email is already taken.");
                    await _logService.LogActionAsync("ValidationError", null, $"Validation failed for UpdateUser: Email {dto.Email} is already taken.");
                    throw new ValidationException(string.Join("; ", validationErrors));
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var user = await _userManager.FindByIdAsync(dto.Id.ToString());
                    if (user == null)
                    {
                        await _logService.LogActionAsync("ValidationError", null, $"User with ID {dto.Id} not found.");
                        throw new ValidationException(ErrorMessages.UserNotFound);
                    }

                    _mapper.Map(dto, user);
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(e => e.Description).ToList();
                        await _logService.LogActionAsync("ValidationError", null, $"Identity validation failed: {string.Join("; ", errors)}");
                        throw new ValidationException(string.Join("; ", errors));
                    }

                    await _logService.LogActionAsync("UserUpdated", user.Id.ToString(), $"User {dto.Email} updated at {DateTime.UtcNow}.");
                });
            }

            public async Task<UserDto> GetUserByIdAsync(string id)
            {
                Guid userId;
                if (!Guid.TryParse(id, out userId))
                {
                    await _logService.LogActionAsync("ValidationError", null, $"Invalid user ID format: {id}.");
                    throw new ValidationException("Invalid user ID format.");
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    await _logService.LogActionAsync("ValidationError", null, $"User with ID {id} not found.");
                    throw new ValidationException(ErrorMessages.UserNotFound);
                }

                return _mapper.Map<UserDto>(user);
            }

            public async Task<UserDto> GetUserByIdAsync(Guid userId)
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    await _logService.LogActionAsync("ValidationError", null, $"User with ID {userId} not found.");
                    throw new ValidationException(ErrorMessages.UserNotFound);
                }

                return _mapper.Map<UserDto>(user);
            }

            public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    await _logService.LogActionAsync("ValidationError", null, $"User with ID {userId} not found.");
                    throw new ValidationException(ErrorMessages.UserNotFound);
                }

                var roles = await _userManager.GetRolesAsync(user);
                await _logService.LogActionAsync("RolesRetrieved", userId.ToString(), $"Roles retrieved for user {user.Email} at {DateTime.UtcNow}.");
                return roles;
            }

            public async Task AssignRoleAsync(Guid userId, string roleName)
            {
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    await _logService.LogActionAsync("ValidationError", null, "Role name is required.");
                    throw new ValidationException("Role name is required.");
                }

                var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
                if (user == null)
                {
                    await _logService.LogActionAsync("ValidationError", null, $"User with ID {userId} not found.");
                    throw new ValidationException(ErrorMessages.UserNotFound);
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        var roleResult = await _roleManager.CreateAsync(new Role { Name = roleName });
                        if (!roleResult.Succeeded)
                        {
                            var errors = roleResult.Errors.Select(e => e.Description).ToList();
                            await _logService.LogActionAsync("ValidationError", null, $"Role creation failed: {string.Join("; ", errors)}");
                            throw new ValidationException(string.Join("; ", errors));
                        }
                    }

                    var result = await _userManager.AddToRoleAsync(user, roleName);
                    if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(e => e.Description).ToList();
                        await _logService.LogActionAsync("ValidationError", null, $"Role assignment failed: {string.Join("; ", errors)}");
                        throw new ValidationException(string.Join("; ", errors));
                    }

                    await _unitOfWork.Repository<LogEntry>().AddAsync(new LogEntry
                    {
                        Action = "RoleAssigned",
                        UserId = user.Id.ToString(),
                        Details = $"Role {roleName} assigned to user {user.Email} at {DateTime.UtcNow}."
                    });
                });
            }

            public async Task<IEnumerable<LogEntryDto>> GetUserLogsAsync(string userId)
            {
                if (!Guid.TryParse(userId, out var parsedUserId))
                {
                    await _logService.LogActionAsync("ValidationError", null, $"Invalid user ID format: {userId}.");
                    throw new ValidationException("Invalid user ID format.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    await _logService.LogActionAsync("ValidationError", null, $"User with ID {userId} not found.");
                    throw new ValidationException(ErrorMessages.UserNotFound);
                }

                var logs = await _unitOfWork.Repository<LogEntry>()
                    .FindAsync(l => l.UserId == userId);
                await _logService.LogActionAsync("LogsRetrieved", userId, $"Logs retrieved for user {user.Email} at {DateTime.UtcNow}.");
                return _mapper.Map<IEnumerable<LogEntryDto>>(logs);
            }

            private List<string> ValidateDto<T>(T dto)
            {
                var context = new ValidationContext(dto, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();
                Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
                return results.Select(r => r.ErrorMessage).ToList()!;
            }
        }
    }
