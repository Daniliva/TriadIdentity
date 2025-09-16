using Microsoft.AspNetCore.Identity;
using TriadIdentity.DAL.Interfaces;


namespace TriadIdentity.DAL.Entities.Identity
{
    public class User : IdentityUser<Guid>, IAuditEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
