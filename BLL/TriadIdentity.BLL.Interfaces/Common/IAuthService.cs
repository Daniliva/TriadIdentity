using TriadIdentity.BLL.Models.DTOs.Identity;

namespace TriadIdentity.BLL.Interfaces.Common
{
	public interface IAuthService
    {
        Task RegisterUserAsync(RegisterUserDto dto);
        Task<string> LoginUserAsync(LoginUserDto dto);
        Task<string> ExternalLoginAsync(string provider, string token);
    }
}
