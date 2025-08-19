using TriadIdentity.BLL.Models.Identity;

namespace TriadIdentity.BLL.Interfaces.Common;

public interface IIdentityService
{
    Task RegisterUserAsync(RegisterUserDto dto);
    Task<string> LoginUserAsync(LoginUserDto dto);
    Task UpdateUserAsync(UpdateUserDto dto);
    Task AssignRoleAsync(Guid userId, string roleName);
    Task<UserDto> GetUserByIdAsync(Guid userId);
    Task<UserDto> GetUserByIdAsync(string email);
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);
    Task<IEnumerable<LogEntryDto>> GetUserLogsAsync(string userId);
}
