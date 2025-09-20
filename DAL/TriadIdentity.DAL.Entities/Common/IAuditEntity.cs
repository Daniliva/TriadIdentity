namespace TriadIdentity.DAL.Entities.Common
{
    public interface IAuditEntity
    {
        DateTime CreatedAt { get; set; }

        DateTime? UpdatedAt { get; set; }
    }
}