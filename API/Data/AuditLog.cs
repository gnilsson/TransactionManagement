using System.ComponentModel.DataAnnotations;

namespace API.Data;

public class AuditLog
{
    [Key]
    public int Id { get; }

    public required string TableName { get; set; }

    public required string Action { get; set; }

    public required string KeyValues { get; set; }

    public required string UserId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public DateTime Timestamp { get; }
}
