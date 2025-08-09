using TriadIdentity.BLL.Models.Identity;

namespace TriadIdentity.BLL.Interfaces.Common;

public interface IIdentityService
{
    Task RegisterUserAsync(RegisterUserDto dto);
    Task<string> LoginUserAsync(LoginUserDto dto);
    Task UpdateUserAsync(UpdateUserDto dto);
    Task AssignRoleAsync(Guid userId, string roleName);
}