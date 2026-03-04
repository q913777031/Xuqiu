using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.ViewModels;

public partial class TaskEditDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _owner;

    [ObservableProperty]
    private string? _estimate;

    [ObservableProperty]
    private TaskItemStatus _status = TaskItemStatus.NotStarted;

    [ObservableProperty]
    private string? _blocker;

    [ObservableProperty]
    private QuadrantType _quadrant;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _dialogTitle = "新增任务";

    [ObservableProperty]
    private string? _titleError;

    public bool? DialogResult { get; private set; }

    public Array StatusValues => Enum.GetValues(typeof(TaskItemStatus));
    public Array QuadrantValues => Enum.GetValues(typeof(QuadrantType));

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
    }

    public void SetQuadrant(QuadrantType quadrant)
    {
        Quadrant = quadrant;
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
