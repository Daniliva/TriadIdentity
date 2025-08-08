namespace TriadIdentity.DAL.Interfaces;

public interface IAuditEntity
{
    DateTime CreatedAt { get; set; }

    DateTime? UpdatedAt { get; set; }
}