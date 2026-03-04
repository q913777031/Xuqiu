using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EisenhowerMatrix.Models;
using EisenhowerMatrix.Services;

namespace EisenhowerMatrix.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly TaskService _taskService;

    public MainViewModel(TaskService taskService)
    {
        _taskService = taskService;
        LoadTasks();
    }

    // Four quadrant collections
    public ObservableCollection<TaskItemViewModel> Q1Tasks { get; } = new();
    public ObservableCollection<TaskItemViewModel> Q2Tasks { get; } = new();
    public ObservableCollection<TaskItemViewModel> Q3Tasks { get; } = new();
    public ObservableCollection<TaskItemViewModel> Q4Tasks { get; } = new();

    // All tasks for list view
    public ObservableCollection<TaskItemViewModel> AllTasks { get; } = new();

    // Filter
    [ObservableProperty]
    private TaskItemStatus? _selectedFilter;

    [ObservableProperty]
    private bool _isQuadrantView = true;

    // Statistics
    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _inProgressCount;

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private int _blockedCount;

    // Quadrant counts
    [ObservableProperty]
    private int _q1Count;

    [ObservableProperty]
    private int _q2Count;

    [ObservableProperty]
    private int _q3Count;

    [ObservableProperty]
    private int _q4Count;

    // Dialog interaction
    public Func<TaskEditDialogViewModel, bool?>? ShowEditDialog { get; set; }
    public Func<string, bool>? ShowConfirmDialog { get; set; }

    public void LoadTasks()
    {
        var allItems = _taskService.GetAllTasks();

        Q1Tasks.Clear();
        Q2Tasks.Clear();
        Q3Tasks.Clear();
        Q4Tasks.Clear();
        AllTasks.Clear();

        foreach (var item in allItems)
        {
            var vm = CreateTaskVm(item);

            if (SelectedFilter.HasValue && item.Status != SelectedFilter.Value)
                continue;

            switch (item.Quadrant)
            {
                case QuadrantType.Q1: Q1Tasks.Add(vm); break;
                case QuadrantType.Q2: Q2Tasks.Add(vm); break;
                case QuadrantType.Q3: Q3Tasks.Add(vm); break;
                case QuadrantType.Q4: Q4Tasks.Add(vm); break;
            }

            AllTasks.Add(vm);
        }

        UpdateStatistics(allItems);
    }

    private TaskItemViewModel CreateTaskVm(TaskItem item)
    {
        var vm = new TaskItemViewModel(item)
        {
            EditAction = EditTask,
            DeleteAction = DeleteTask
        };
        return vm;
    }

    private void UpdateStatistics(List<TaskItem>? items = null)
    {
        items ??= _taskService.GetAllTasks();

        TotalCount = items.Count;
        InProgressCount = items.Count(t => t.Status == TaskItemStatus.InProgress);
        CompletedCount = items.Count(t => t.Status == TaskItemStatus.Completed);
        BlockedCount = items.Count(t => t.Status == TaskItemStatus.Blocked);

        Q1Count = Q1Tasks.Count;
        Q2Count = Q2Tasks.Count;
        Q3Count = Q3Tasks.Count;
        Q4Count = Q4Tasks.Count;
    }

    [RelayCommand]
    private void AddTask(QuadrantType quadrant)
    {
        var dialogVm = new TaskEditDialogViewModel();
        dialogVm.SetQuadrant(quadrant);

        var result = ShowEditDialog?.Invoke(dialogVm);
        if (result != true) return;

        var task = new TaskItem
        {
            Title = dialogVm.Title,
            Owner = dialogVm.Owner,
            Estimate = dialogVm.Estimate,
            Status = dialogVm.Status,
            Blocker = dialogVm.Blocker,
            Quadrant = dialogVm.Quadrant
        };

        _taskService.AddTask(task);
        LoadTasks();
    }

    public void EditTask(TaskItemViewModel taskVm)
    {
        var dialogVm = new TaskEditDialogViewModel();
        dialogVm.LoadFromTask(taskVm);

        var result = ShowEditDialog?.Invoke(dialogVm);
        if (result != true) return;

        taskVm.Model.Title = dialogVm.Title;
        taskVm.Model.Owner = dialogVm.Owner;
        taskVm.Model.Estimate = dialogVm.Estimate;
        taskVm.Model.Status = dialogVm.Status;
        taskVm.Model.Blocker = dialogVm.Blocker;
        taskVm.Model.Quadrant = dialogVm.Quadrant;

        _taskService.UpdateTask(taskVm.Model);
        LoadTasks();
    }

    public void DeleteTask(TaskItemViewModel taskVm)
    {
        var confirmed = ShowConfirmDialog?.Invoke($"确定要删除任务「{taskVm.Title}」吗？");
        if (confirmed != true) return;

        _taskService.DeleteTask(taskVm.Id);
        LoadTasks();
    }

    [RelayCommand]
    private void SetFilter(string? statusStr)
    {
        if (string.IsNullOrEmpty(statusStr))
        {
            SelectedFilter = null;
        }
        else if (Enum.TryParse<TaskItemStatus>(statusStr, out var status))
        {
            SelectedFilter = SelectedFilter == status ? null : status;
        }
        LoadTasks();
    }

    [RelayCommand]
    private void ToggleView()
    {
        IsQuadrantView = !IsQuadrantView;
    }

    public void MoveTask(TaskItemViewModel taskVm, QuadrantType targetQuadrant)
    {
        taskVm.Model.Quadrant = targetQuadrant;
        _taskService.UpdateTask(taskVm.Model);
        LoadTasks();
    }

    public void UpdateTaskStatus(TaskItemViewModel taskVm, TaskItemStatus newStatus)
    {
        taskVm.Model.Status = newStatus;
        _taskService.UpdateTask(taskVm.Model);
        LoadTasks();
    }

    public void ReorderTasks(QuadrantType quadrant, ObservableCollection<TaskItemViewModel> tasks)
    {
        var itemsToUpdate = new List<TaskItem>();
        for (int i = 0; i < tasks.Count; i++)
        {
            tasks[i].Model.SortOrder = i;
            tasks[i].Model.Quadrant = quadrant;
            itemsToUpdate.Add(tasks[i].Model);
        }
        _taskService.UpdateSortOrders(itemsToUpdate);
    }

    public ObservableCollection<TaskItemViewModel> GetQuadrantCollection(QuadrantType quadrant)
    {
        return quadrant switch
        {
            QuadrantType.Q1 => Q1Tasks,
            QuadrantType.Q2 => Q2Tasks,
            QuadrantType.Q3 => Q3Tasks,
            QuadrantType.Q4 => Q4Tasks,
            _ => Q1Tasks
        };
    }
}
