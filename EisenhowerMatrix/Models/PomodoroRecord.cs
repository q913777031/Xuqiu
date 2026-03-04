using FreeSql.DataAnnotations;

namespace EisenhowerMatrix.Models;

[Table(Name = "PomodoroRecord")]
public class PomodoroRecord
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public int TaskId { get; set; }

    [Column(IsNullable = false)]
    public DateTime StartTime { get; set; }

    [Column(IsNullable = false)]
    public DateTime EndTime { get; set; }

    public int Duration { get; set; }

    public int Completed { get; set; } = 1;

    [Navigate(nameof(TaskId))]
    public TaskItem? Task { get; set; }
}
