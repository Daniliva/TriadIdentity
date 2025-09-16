using Microsoft.AspNetCore.Identity;
using TriadIdentity.DAL.Interfaces;

namespace TriadIdentity.DAL.Entities.Identity
{
    public class UserToken : IdentityUserToken<Guid>, IAuditEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}