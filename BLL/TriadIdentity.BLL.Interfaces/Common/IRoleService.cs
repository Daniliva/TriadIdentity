namespace TriadIdentity.BLL.Interfaces.Common
{
	public interface IRoleService
    {
        Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);
        Task AssignRoleAsync(Guid userId, string roleName);
        Task RemoveRoleAsync(Guid userId, string roleName);
    }
}
