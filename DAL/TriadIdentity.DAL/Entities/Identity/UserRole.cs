using Microsoft.AspNetCore.Identity;
using TriadIdentity.DAL.Interfaces;

namespace TriadIdentity.DAL.Entities.Identity;

public class UserRole : IdentityUserRole<Guid>, IAuditEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}