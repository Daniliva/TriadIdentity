using Microsoft.AspNetCore.Identity;
using TriadIdentity.DAL.Entities.Common;

namespace TriadIdentity.DAL.Entities.Identity;

public class UserClaim : IdentityUserClaim<Guid>, IAuditEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}