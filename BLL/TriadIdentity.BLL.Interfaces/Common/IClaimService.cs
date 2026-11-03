using TriadIdentity.BLL.Models.DTOs.Identity;

namespace TriadIdentity.BLL.Interfaces.Common
{
	public interface IClaimService
    {
        Task<IEnumerable<UserClaimDto>> GetUserClaimsAsync(Guid userId);
        Task AddUserClaimAsync(UserClaimDto dto);
        Task RemoveUserClaimAsync(Guid userId, string claimType, string claimValue);
    }
}
