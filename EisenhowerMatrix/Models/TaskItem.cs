using FreeSql.DataAnnotations;

namespace EisenhowerMatrix.Models;

[Table(Name = "TaskItem")]
public class TaskItem
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    [Column(IsNullable = false)]
    public string Title { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public QuadrantType Quadrant { get; set; } = QuadrantType.Q1;

    public string? Owner { get; set; }

    public string? Estimate { get; set; }

    [Column(IsNullable = false)]
    public TaskItemStatus Status { get; set; } = TaskItemStatus.NotStarted;

    public string? Blocker { get; set; }

    [Column(IsNullable = false)]
    public int SortOrder { get; set; } = 0;

    [Column(IsNullable = false)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column(IsNullable = false)]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // v2.0
    public int BoardId { get; set; } = 1;

    public int? ParentId { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? ReminderTime { get; set; }

    public int TotalFocusTime { get; set; } = 0;

    public int PomodoroCount { get; set; } = 0;

    public int IsArchived { get; set; } = 0;

    [Navigate(nameof(ParentId))]
    public TaskItem? Parent { get; set; }

    [Navigate(nameof(ParentId))]
    public List<TaskItem>? Children { get; set; }

    [Navigate(ManyToMany = typeof(TaskTag))]
    public List<Tag>? Tags { get; set; }
}
