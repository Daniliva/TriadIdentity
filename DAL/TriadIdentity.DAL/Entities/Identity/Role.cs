using Microsoft.AspNetCore.Identity;

namespace TriadIdentity.DAL.Entities.Identity;

public class Role : IdentityRole<Guid>
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}