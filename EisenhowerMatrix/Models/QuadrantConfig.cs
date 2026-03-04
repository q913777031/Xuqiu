using FreeSql.DataAnnotations;

namespace EisenhowerMatrix.Models;

[Table(Name = "QuadrantConfig")]
public class QuadrantConfig
{
    public QuadrantType Quadrant { get; set; }

    public int BoardId { get; set; } = 1;

    [Column(IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public string ActionHint { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public string Color { get; set; } = "#3B82F6";
}
