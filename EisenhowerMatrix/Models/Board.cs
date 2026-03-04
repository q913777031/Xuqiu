using FreeSql.DataAnnotations;

namespace EisenhowerMatrix.Models;

[Table(Name = "Board")]
public class Board
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    [Column(IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Color { get; set; } = "#3B82F6";

    public int SortOrder { get; set; } = 0;

    [Column(IsNullable = false)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int IsArchived { get; set; } = 0;
}
