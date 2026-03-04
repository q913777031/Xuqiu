using FreeSql.DataAnnotations;

namespace EisenhowerMatrix.Models;

[Table(Name = "Tag")]
public class Tag
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    [Column(IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = "#3B82F6";

    [Navigate(ManyToMany = typeof(TaskTag))]
    public List<TaskItem>? Tasks { get; set; }
}
