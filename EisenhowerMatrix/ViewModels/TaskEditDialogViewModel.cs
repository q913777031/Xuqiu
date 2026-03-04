using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.ViewModels;

public class TagSelectionItem : ObservableObject
{
    public Tag Tag { get; set; } = null!;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }
}

public partial class TaskEditDialogViewModel : ObservableObject
{
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string? _owner;
    [ObservableProperty] private string? _estimate;
    [ObservableProperty] private TaskItemStatus _status = TaskItemStatus.NotStarted;
    [ObservableProperty] private string? _blocker;
    [ObservableProperty] private QuadrantType _quadrant;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private string _dialogTitle = "新增任务";
    [ObservableProperty] private string? _titleError;
    [ObservableProperty] private DateTime? _dueDate;
    [ObservableProperty] private string _newSubtaskTitle = "";

    public ObservableCollection<TagSelectionItem> TagItems { get; } = new();
    public ObservableCollection<string> SubtaskTitles { get; } = new();
    public ObservableCollection<TaskTemplate> AvailableTemplates { get; } = new();

    public List<int> SelectedTagIds =>
        TagItems.Where(t => t.IsSelected).Select(t => t.Tag.Id).ToList();

    public bool? DialogResult { get; private set; }

    public Array StatusValues => Enum.GetValues(typeof(TaskItemStatus));
    public Array QuadrantValues => Enum.GetValues(typeof(QuadrantType));

    public TaskEditDialogViewModel(List<Tag> allTags, List<TaskTemplate> templates)
    {
        foreach (var tag in allTags)
            TagItems.Add(new TagSelectionItem { Tag = tag });

        foreach (var t in templates)
            AvailableTemplates.Add(t);
    }

    public static string GetStatusDisplayName(TaskItemStatus status) => status switch
    {
        TaskItemStatus.NotStarted => "未开始",
        TaskItemStatus.InProgress => "进行中",
        TaskItemStatus.Completed => "已完成",
        TaskItemStatus.Blocked => "阻塞",
        _ => ""
    };

    public static string GetQuadrantDisplayName(QuadrantType quadrant) => quadrant switch
    {
        QuadrantType.Q1 => "Q1 重要&紧急",
        QuadrantType.Q2 => "Q2 重要&不紧急",
        QuadrantType.Q3 => "Q3 不重要&紧急",
        QuadrantType.Q4 => "Q4 不重要&不紧急",
        _ => ""
    };

    public void LoadFromTask(TaskItemViewModel task)
    {
        IsEditMode = true;
        DialogTitle = "编辑任务";
        Title = task.Title;
        Owner = task.Owner;
        Estimate = task.Estimate;
        Status = task.Status;
        Blocker = task.Blocker;
        Quadrant = task.Quadrant;
        DueDate = task.DueDate;

        // Set tag selections
        foreach (var tagItem in TagItems)
        {
            tagItem.IsSelected = task.TagsList.Any(t => t.Id == tagItem.Tag.Id);
        }
    }

    public void SetQuadrant(QuadrantType quadrant)
    {
        Quadrant = quadrant;
    }

    [RelayCommand]
    private void ApplyTemplate(TaskTemplate? template)
    {
        if (template == null) return;

        Title = template.Title;
        Quadrant = template.Quadrant;
        Owner = string.IsNullOrWhiteSpace(template.Owner) ? null : template.Owner;
        Estimate = string.IsNullOrWhiteSpace(template.Estimate) ? null : template.Estimate;

        // Parse subtask titles from JSON
        if (!string.IsNullOrWhiteSpace(template.SubtaskTitles))
        {
            try
            {
                var subtasks = System.Text.Json.JsonSerializer.Deserialize<List<string>>(template.SubtaskTitles);
                if (subtasks != null)
                {
                    SubtaskTitles.Clear();
                    foreach (var st in subtasks)
                        SubtaskTitles.Add(st);
                }
            }
            catch { }
        }
    }

    [RelayCommand]
    private void AddSubtask()
    {
        if (!string.IsNullOrWhiteSpace(NewSubtaskTitle))
        {
            SubtaskTitles.Add(NewSubtaskTitle.Trim());
            NewSubtaskTitle = "";
        }
    }

    [RelayCommand]
    private void RemoveSubtask(string? subtask)
    {
        if (subtask != null)
            SubtaskTitles.Remove(subtask);
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            TitleError = "标题不能为空";
            return;
        }
        TitleError = null;
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
    }
}
