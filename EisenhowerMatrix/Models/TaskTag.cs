using FreeSql.DataAnnotations;

namespace EisenhowerMatrix.Models;

[Table(Name = "TaskTag")]
public class TaskTag
{
    public int TaskId { get; set; }
    public int TagId { get; set; }

    [Navigate(nameof(TaskId))]
    public TaskItem? Task { get; set; }

    [Navigate(nameof(TagId))]
    public Tag? Tag { get; set; }
}
