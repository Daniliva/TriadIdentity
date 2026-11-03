using TriadIdentity.BLL.Models.DTOs.Identity;

namespace TriadIdentity.BLL.Interfaces.Common
{
	public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task UpdateUserAsync(UpdateUserDto dto);
    }
}
