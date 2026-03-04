using FreeSql.DataAnnotations;

namespace EisenhowerMatrix.Models;

[Table(Name = "OperationLog")]
public class OperationLog
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    [Column(IsNullable = false)]
    public string OperationType { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    [Column(IsNullable = false)]
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
