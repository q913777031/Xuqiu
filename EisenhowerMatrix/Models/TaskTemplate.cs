using FreeSql.DataAnnotations;

namespace EisenhowerMatrix.Models;

[Table(Name = "TaskTemplate")]
public class TaskTemplate
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    [Column(IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public string Title { get; set; } = string.Empty;

    public QuadrantType Quadrant { get; set; } = QuadrantType.Q1;

    public string? Owner { get; set; }

    public string? Estimate { get; set; }

    public string? Tags { get; set; }

    public string? SubtaskTitles { get; set; }
}
