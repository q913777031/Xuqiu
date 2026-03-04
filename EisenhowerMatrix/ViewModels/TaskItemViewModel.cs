using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.ViewModels;

public partial class TaskItemViewModel : ObservableObject
{
    private readonly TaskItem _model;

    public TaskItemViewModel(TaskItem model)
    {
        _model = model;
    }

    public TaskItem Model => _model;

    public int Id => _model.Id;

    public string Title
    {
        get => _model.Title;
        set { _model.Title = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsCompleted)); }
    }

    public QuadrantType Quadrant
    {
        get => _model.Quadrant;
        set { _model.Quadrant = value; OnPropertyChanged(); }
    }

    public string? Owner
    {
        get => _model.Owner;
        set { _model.Owner = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasOwner)); }
    }

    public bool HasOwner => !string.IsNullOrWhiteSpace(Owner);

    public string? Estimate
    {
        get => _model.Estimate;
        set { _model.Estimate = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasEstimate)); }
    }

    public bool HasEstimate => !string.IsNullOrWhiteSpace(Estimate);

    public TaskItemStatus Status
    {
        get => _model.Status;
        set
        {
            _model.Status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsBlocked));
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public bool IsCompleted => Status == TaskItemStatus.Completed;
    public bool IsBlocked => Status == TaskItemStatus.Blocked;

    public string StatusText => Status switch
    {
        TaskItemStatus.NotStarted => "未开始",
        TaskItemStatus.InProgress => "进行中",
        TaskItemStatus.Completed => "已完成",
        TaskItemStatus.Blocked => "阻塞",
        _ => ""
    };

    public string? Blocker
    {
        get => _model.Blocker;
        set { _model.Blocker = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasBlocker)); }
    }

    public bool HasBlocker => IsBlocked && !string.IsNullOrWhiteSpace(Blocker);

    public int SortOrder
    {
        get => _model.SortOrder;
        set { _model.SortOrder = value; OnPropertyChanged(); }
    }

    public DateTime CreatedAt => _model.CreatedAt;

    // === v2.0 Properties ===

    public DateTime? DueDate
    {
        get => _model.DueDate;
        set { _model.DueDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDueDate)); OnPropertyChanged(nameof(IsOverdue)); OnPropertyChanged(nameof(DueDateText)); }
    }

    public bool HasDueDate => DueDate.HasValue;

    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.Now && Status != TaskItemStatus.Completed;

    public string DueDateText => DueDate?.ToString("MM-dd") ?? "";

    public int PomodoroCount
    {
        get => _model.PomodoroCount;
        set { _model.PomodoroCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPomodoro)); }
    }

    public bool HasPomodoro => PomodoroCount > 0;

    [ObservableProperty] private ObservableCollection<Tag> _tagsList = new();

    public bool HasTags => TagsList.Count > 0;

    [ObservableProperty] private int _subtaskTotal;
    [ObservableProperty] private int _subtaskCompleted;

    public bool HasSubtasks => SubtaskTotal > 0;
    public string SubtaskText => $"{SubtaskCompleted}/{SubtaskTotal}";

    partial void OnTagsListChanged(ObservableCollection<Tag> value)
    {
        OnPropertyChanged(nameof(HasTags));
    }

    partial void OnSubtaskTotalChanged(int value)
    {
        OnPropertyChanged(nameof(HasSubtasks));
        OnPropertyChanged(nameof(SubtaskText));
    }

    partial void OnSubtaskCompletedChanged(int value)
    {
        OnPropertyChanged(nameof(SubtaskText));
    }

    // Commands are set by MainViewModel
    public Action<TaskItemViewModel>? EditAction { get; set; }
    public Action<TaskItemViewModel>? DeleteAction { get; set; }
    public Action<TaskItemViewModel>? StartPomodoroAction { get; set; }

    [RelayCommand]
    private void Edit() => EditAction?.Invoke(this);

    [RelayCommand]
    private void Delete() => DeleteAction?.Invoke(this);

    [RelayCommand]
    private void StartPomodoro() => StartPomodoroAction?.Invoke(this);
}
