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

    // Commands are set by MainViewModel
    public Action<TaskItemViewModel>? EditAction { get; set; }
    public Action<TaskItemViewModel>? DeleteAction { get; set; }

    [RelayCommand]
    private void Edit() => EditAction?.Invoke(this);

    [RelayCommand]
    private void Delete() => DeleteAction?.Invoke(this);
}
