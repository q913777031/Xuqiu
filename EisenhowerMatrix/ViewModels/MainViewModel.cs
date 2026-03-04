using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EisenhowerMatrix.Models;
using EisenhowerMatrix.Services;
using Microsoft.Win32;

namespace EisenhowerMatrix.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly TaskService _taskService;
    private readonly BoardService _boardService;
    private readonly TagService _tagService;
    private readonly PomodoroService _pomodoroService;
    private readonly SettingsService _settings;
    private readonly TemplateService _templateService;
    private readonly UndoRedoService _undoRedo;
    private readonly CsvService _csvService;
    private readonly AnalyticsService _analyticsService;

    // Pomodoro timer
    private DispatcherTimer? _pomodoroTimer;
    private DateTime _pomodoroStartTime;
    private int _pomodoroTaskId;

    public MainViewModel(TaskService taskService, BoardService boardService, TagService tagService,
        PomodoroService pomodoroService, SettingsService settings, TemplateService templateService,
        UndoRedoService undoRedo, CsvService csvService, AnalyticsService analyticsService)
    {
        _taskService = taskService;
        _boardService = boardService;
        _tagService = tagService;
        _pomodoroService = pomodoroService;
        _settings = settings;
        _templateService = templateService;
        _undoRedo = undoRedo;
        _csvService = csvService;
        _analyticsService = analyticsService;

        _undoRedo.StateChanged += () =>
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        };

        CurrentBoardId = _settings.LastBoardId;
        IsDarkMode = _settings.Theme == "Dark";
        LoadBoards();
        LoadTags();
        LoadTasks();
        LoadTemplates();
    }

    // === Collections ===
    public ObservableCollection<TaskItemViewModel> Q1Tasks { get; } = new();
    public ObservableCollection<TaskItemViewModel> Q2Tasks { get; } = new();
    public ObservableCollection<TaskItemViewModel> Q3Tasks { get; } = new();
    public ObservableCollection<TaskItemViewModel> Q4Tasks { get; } = new();
    public ObservableCollection<TaskItemViewModel> AllTasks { get; } = new();
    public ObservableCollection<Board> Boards { get; } = new();
    public ObservableCollection<Tag> AllTags { get; } = new();
    public ObservableCollection<TaskTemplate> Templates { get; } = new();

    // === Analytics ===
    public ObservableCollection<QuadrantStat> QuadrantStats { get; } = new();
    public ObservableCollection<DailyCompletionStat> CompletionTrend { get; } = new();
    public ObservableCollection<OwnerWorkloadStat> OwnerWorkload { get; } = new();

    // === State ===
    [ObservableProperty] private TaskItemStatus? _selectedFilter;
    [ObservableProperty] private bool _isQuadrantView = true;
    [ObservableProperty] private bool _isListView;
    [ObservableProperty] private bool _isAnalyticsView;
    [ObservableProperty] private int _currentBoardId = 1;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private bool _isDarkMode;
    [ObservableProperty] private Tag? _selectedTagFilter;

    // === Statistics ===
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _inProgressCount;
    [ObservableProperty] private int _completedCount;
    [ObservableProperty] private int _blockedCount;
    [ObservableProperty] private int _overdueCount;
    [ObservableProperty] private int _q1Count;
    [ObservableProperty] private int _q2Count;
    [ObservableProperty] private int _q3Count;
    [ObservableProperty] private int _q4Count;
    [ObservableProperty] private double _q2Percentage;

    // === Pomodoro ===
    [ObservableProperty] private bool _isPomodoroRunning;
    [ObservableProperty] private string _pomodoroTimeDisplay = "25:00";
    [ObservableProperty] private string _pomodoroTaskTitle = "";
    [ObservableProperty] private int _todayPomodoroCount;
    private int _pomodoroSecondsLeft;
    private bool _isPomodoroPaused;

    // === Undo/Redo ===
    public bool CanUndo => _undoRedo.CanUndo;
    public bool CanRedo => _undoRedo.CanRedo;

    // === Dialog Interactions ===
    public Func<TaskEditDialogViewModel, bool?>? ShowEditDialog { get; set; }
    public Func<string, bool>? ShowConfirmDialog { get; set; }
    public Action<string>? ShowMessage { get; set; }

    // ====================== LOAD DATA ======================

    public void LoadBoards()
    {
        Boards.Clear();
        foreach (var b in _boardService.GetAllBoards())
            Boards.Add(b);
    }

    public void LoadTags()
    {
        AllTags.Clear();
        foreach (var t in _tagService.GetAllTags())
            AllTags.Add(t);
    }

    public void LoadTemplates()
    {
        Templates.Clear();
        foreach (var t in _templateService.GetAllTemplates())
            Templates.Add(t);
    }

    public void LoadTasks()
    {
        List<TaskItem> allItems;

        if (!string.IsNullOrWhiteSpace(SearchText))
            allItems = _taskService.SearchTasks(SearchText, CurrentBoardId);
        else
            allItems = _taskService.GetAllTasks(CurrentBoardId);

        Q1Tasks.Clear();
        Q2Tasks.Clear();
        Q3Tasks.Clear();
        Q4Tasks.Clear();
        AllTasks.Clear();

        foreach (var item in allItems)
        {
            if (SelectedFilter.HasValue && item.Status != SelectedFilter.Value)
                continue;

            if (SelectedTagFilter != null)
            {
                var taskTags = _tagService.GetTagsForTask(item.Id);
                if (!taskTags.Any(t => t.Id == SelectedTagFilter.Id))
                    continue;
            }

            var vm = CreateTaskVm(item);

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
        TodayPomodoroCount = _pomodoroService.GetTodayPomodoroCount();
    }

    private TaskItemViewModel CreateTaskVm(TaskItem item)
    {
        var tags = _tagService.GetTagsForTask(item.Id);
        var subtaskCount = _taskService.GetSubtaskCount(item.Id);
        var completedSubtaskCount = _taskService.GetCompletedSubtaskCount(item.Id);

        var vm = new TaskItemViewModel(item)
        {
            EditAction = EditTask,
            DeleteAction = DeleteTask,
            StartPomodoroAction = StartPomodoro,
            TagsList = new ObservableCollection<Tag>(tags),
            SubtaskTotal = subtaskCount,
            SubtaskCompleted = completedSubtaskCount
        };
        return vm;
    }

    private void UpdateStatistics(List<TaskItem>? items = null)
    {
        items ??= _taskService.GetAllTasks(CurrentBoardId);

        TotalCount = items.Count;
        InProgressCount = items.Count(t => t.Status == TaskItemStatus.InProgress);
        CompletedCount = items.Count(t => t.Status == TaskItemStatus.Completed);
        BlockedCount = items.Count(t => t.Status == TaskItemStatus.Blocked);
        OverdueCount = items.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.Now && t.Status != TaskItemStatus.Completed);

        Q1Count = Q1Tasks.Count;
        Q2Count = Q2Tasks.Count;
        Q3Count = Q3Tasks.Count;
        Q4Count = Q4Tasks.Count;
        Q2Percentage = _analyticsService.GetQ2Percentage(CurrentBoardId);
    }

    // ====================== TASK CRUD ======================

    [RelayCommand]
    private void AddTask(QuadrantType quadrant)
    {
        var dialogVm = new TaskEditDialogViewModel(_tagService.GetAllTags(), _templateService.GetAllTemplates());
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
            Quadrant = dialogVm.Quadrant,
            DueDate = dialogVm.DueDate,
            BoardId = CurrentBoardId
        };

        var added = _taskService.AddTask(task);

        // Save tags
        if (dialogVm.SelectedTagIds.Count > 0)
            _tagService.SetTaskTags(added.Id, dialogVm.SelectedTagIds);

        // Save subtasks
        foreach (var stTitle in dialogVm.SubtaskTitles)
        {
            if (!string.IsNullOrWhiteSpace(stTitle))
            {
                _taskService.AddTask(new TaskItem
                {
                    Title = stTitle,
                    ParentId = added.Id,
                    Quadrant = added.Quadrant,
                    BoardId = CurrentBoardId
                });
            }
        }

        _undoRedo.RecordAction("新增任务",
            () => { _taskService.DeleteTask(added.Id); LoadTasks(); },
            () => { _taskService.AddTask(task); LoadTasks(); });

        LoadTasks();
    }

    public void EditTask(TaskItemViewModel taskVm)
    {
        var dialogVm = new TaskEditDialogViewModel(_tagService.GetAllTags(), _templateService.GetAllTemplates());
        dialogVm.LoadFromTask(taskVm);

        var result = ShowEditDialog?.Invoke(dialogVm);
        if (result != true) return;

        // Snapshot for undo
        var before = _taskService.GetTaskById(taskVm.Id);
        var beforeJson = System.Text.Json.JsonSerializer.Serialize(before);

        taskVm.Model.Title = dialogVm.Title;
        taskVm.Model.Owner = dialogVm.Owner;
        taskVm.Model.Estimate = dialogVm.Estimate;
        taskVm.Model.Status = dialogVm.Status;
        taskVm.Model.Blocker = dialogVm.Blocker;
        taskVm.Model.Quadrant = dialogVm.Quadrant;
        taskVm.Model.DueDate = dialogVm.DueDate;

        _taskService.UpdateTask(taskVm.Model);
        _tagService.SetTaskTags(taskVm.Id, dialogVm.SelectedTagIds);

        var afterJson = System.Text.Json.JsonSerializer.Serialize(taskVm.Model);
        _undoRedo.RecordAction("编辑任务",
            () =>
            {
                var restored = System.Text.Json.JsonSerializer.Deserialize<TaskItem>(beforeJson);
                if (restored != null) _taskService.UpdateTask(restored);
                LoadTasks();
            },
            () =>
            {
                var current = System.Text.Json.JsonSerializer.Deserialize<TaskItem>(afterJson);
                if (current != null) _taskService.UpdateTask(current);
                LoadTasks();
            });

        LoadTasks();
    }

    public void DeleteTask(TaskItemViewModel taskVm)
    {
        var confirmed = ShowConfirmDialog?.Invoke($"确定要删除任务「{taskVm.Title}」吗？");
        if (confirmed != true) return;

        var snapshot = System.Text.Json.JsonSerializer.Serialize(taskVm.Model);
        _taskService.DeleteTask(taskVm.Id);

        _undoRedo.RecordAction("删除任务",
            () =>
            {
                var restored = System.Text.Json.JsonSerializer.Deserialize<TaskItem>(snapshot);
                if (restored != null) { restored.Id = 0; _taskService.AddTask(restored); }
                LoadTasks();
            },
            () => { /* already deleted */ });

        LoadTasks();
    }

    // ====================== FILTERS & VIEWS ======================

    [RelayCommand]
    private void SetFilter(string? statusStr)
    {
        if (string.IsNullOrEmpty(statusStr))
            SelectedFilter = null;
        else if (Enum.TryParse<TaskItemStatus>(statusStr, out var status))
            SelectedFilter = SelectedFilter == status ? null : status;
        LoadTasks();
    }

    [RelayCommand]
    private void SetTagFilter(Tag? tag)
    {
        SelectedTagFilter = SelectedTagFilter?.Id == tag?.Id ? null : tag;
        LoadTasks();
    }

    [RelayCommand]
    private void ShowQuadrantView()
    {
        IsQuadrantView = true; IsListView = false; IsAnalyticsView = false;
    }

    [RelayCommand]
    private void ShowListView()
    {
        IsQuadrantView = false; IsListView = true; IsAnalyticsView = false;
    }

    [RelayCommand]
    private void ShowAnalyticsView()
    {
        IsQuadrantView = false; IsListView = false; IsAnalyticsView = true;
        LoadAnalytics();
    }

    [RelayCommand]
    private void ToggleView()
    {
        if (IsQuadrantView) ShowListView();
        else if (IsListView) ShowAnalyticsView();
        else ShowQuadrantView();
    }

    [RelayCommand]
    private void Search(string? text)
    {
        SearchText = text ?? "";
        LoadTasks();
    }

    // ====================== BOARDS ======================

    [RelayCommand]
    private void SwitchBoard(Board? board)
    {
        if (board == null) return;
        CurrentBoardId = board.Id;
        _settings.LastBoardId = board.Id;
        LoadTasks();
    }

    [RelayCommand]
    private void AddBoard()
    {
        var name = "新看板";
        var board = _boardService.AddBoard(new Board { Name = name });
        LoadBoards();
        SwitchBoard(board);
    }

    [RelayCommand]
    private void DeleteBoard(Board? board)
    {
        if (board == null || board.Id == 1) return;
        var confirmed = ShowConfirmDialog?.Invoke($"确定要删除看板「{board.Name}」吗？任务将移至默认看板。");
        if (confirmed != true) return;
        _boardService.DeleteBoard(board.Id);
        if (CurrentBoardId == board.Id) CurrentBoardId = 1;
        LoadBoards();
        LoadTasks();
    }

    // ====================== DARK MODE ======================

    [RelayCommand]
    private void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        if (IsDarkMode)
        {
            AntDesign.WPF.ThemeHelper.SetBaseTheme(AntDesign.WPF.BaseTheme.Dark);
            _settings.Theme = "Dark";
        }
        else
        {
            AntDesign.WPF.ThemeHelper.SetBaseTheme(AntDesign.WPF.BaseTheme.Light);
            _settings.Theme = "Light";
        }
    }

    // ====================== UNDO/REDO ======================

    [RelayCommand]
    private void Undo()
    {
        var desc = _undoRedo.Undo();
        if (desc != null)
        {
            LoadTasks();
            ShowMessage?.Invoke($"已撤销: {desc}");
        }
    }

    [RelayCommand]
    private void Redo()
    {
        var desc = _undoRedo.Redo();
        if (desc != null)
        {
            LoadTasks();
            ShowMessage?.Invoke($"已重做: {desc}");
        }
    }

    // ====================== DRAG & DROP ======================

    public void MoveTask(TaskItemViewModel taskVm, QuadrantType targetQuadrant)
    {
        var oldQuadrant = taskVm.Model.Quadrant;
        taskVm.Model.Quadrant = targetQuadrant;
        _taskService.UpdateTask(taskVm.Model);

        _undoRedo.RecordAction("移动任务",
            () => { taskVm.Model.Quadrant = oldQuadrant; _taskService.UpdateTask(taskVm.Model); LoadTasks(); },
            () => { taskVm.Model.Quadrant = targetQuadrant; _taskService.UpdateTask(taskVm.Model); LoadTasks(); });

        LoadTasks();
    }

    public void UpdateTaskStatus(TaskItemViewModel taskVm, TaskItemStatus newStatus)
    {
        var oldStatus = taskVm.Model.Status;
        taskVm.Model.Status = newStatus;
        _taskService.UpdateTask(taskVm.Model);

        _undoRedo.RecordAction("修改状态",
            () => { taskVm.Model.Status = oldStatus; _taskService.UpdateTask(taskVm.Model); LoadTasks(); },
            () => { taskVm.Model.Status = newStatus; _taskService.UpdateTask(taskVm.Model); LoadTasks(); });

        LoadTasks();
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

    // ====================== POMODORO ======================

    public void StartPomodoro(TaskItemViewModel taskVm)
    {
        if (IsPomodoroRunning) return;

        _pomodoroTaskId = taskVm.Id;
        PomodoroTaskTitle = taskVm.Title;
        _pomodoroSecondsLeft = _settings.PomodoroWorkMinutes * 60;
        _pomodoroStartTime = DateTime.Now;
        _isPomodoroPaused = false;
        IsPomodoroRunning = true;
        UpdatePomodoroDisplay();

        _pomodoroTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _pomodoroTimer.Tick += PomodoroTick;
        _pomodoroTimer.Start();
    }

    private void PomodoroTick(object? sender, EventArgs e)
    {
        if (_isPomodoroPaused) return;

        _pomodoroSecondsLeft--;
        UpdatePomodoroDisplay();

        if (_pomodoroSecondsLeft <= 0)
            CompletePomodoroSession();
    }

    private void UpdatePomodoroDisplay()
    {
        var min = _pomodoroSecondsLeft / 60;
        var sec = _pomodoroSecondsLeft % 60;
        PomodoroTimeDisplay = $"{min:D2}:{sec:D2}";
    }

    private void CompletePomodoroSession()
    {
        _pomodoroTimer?.Stop();

        var record = new PomodoroRecord
        {
            TaskId = _pomodoroTaskId,
            StartTime = _pomodoroStartTime,
            EndTime = DateTime.Now,
            Duration = _settings.PomodoroWorkMinutes * 60,
            Completed = 1
        };
        _pomodoroService.AddRecord(record);

        TodayPomodoroCount = _pomodoroService.GetTodayPomodoroCount();
        IsPomodoroRunning = false;
        PomodoroTimeDisplay = "00:00";
        ShowMessage?.Invoke($"番茄钟完成！休息 {_settings.PomodoroBreakMinutes} 分钟");
        LoadTasks();
    }

    [RelayCommand]
    private void PausePomodoro()
    {
        _isPomodoroPaused = !_isPomodoroPaused;
    }

    [RelayCommand]
    private void StopPomodoro()
    {
        _pomodoroTimer?.Stop();
        IsPomodoroRunning = false;
        PomodoroTimeDisplay = $"{_settings.PomodoroWorkMinutes:D2}:00";
    }

    // ====================== CSV ======================

    [RelayCommand]
    private void ExportCsv()
    {
        var tasks = _taskService.GetAllTasks(CurrentBoardId);
        var csv = _csvService.ExportToCsv(tasks, _tagService);

        var dialog = new SaveFileDialog
        {
            Filter = "CSV 文件|*.csv",
            FileName = $"eisenhower_export_{DateTime.Now:yyyyMMdd}.csv"
        };
        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, csv, System.Text.Encoding.UTF8);
            ShowMessage?.Invoke($"已导出 {tasks.Count} 个任务");
        }
    }

    [RelayCommand]
    private void ImportCsv()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV 文件|*.csv"
        };
        if (dialog.ShowDialog() != true) return;

        var content = File.ReadAllText(dialog.FileName, System.Text.Encoding.UTF8);
        var tasks = _csvService.ParseCsv(content, CurrentBoardId);

        if (tasks.Count == 0)
        {
            ShowMessage?.Invoke("未找到有效任务数据");
            return;
        }

        var confirmed = ShowConfirmDialog?.Invoke($"将导入 {tasks.Count} 个任务到当前看板，是否继续？");
        if (confirmed != true) return;

        foreach (var task in tasks)
            _taskService.AddTask(task);

        LoadTasks();
        ShowMessage?.Invoke($"已导入 {tasks.Count} 个任务");
    }

    // ====================== ANALYTICS ======================

    public void LoadAnalytics()
    {
        QuadrantStats.Clear();
        foreach (var s in _analyticsService.GetQuadrantDistribution(CurrentBoardId))
            QuadrantStats.Add(s);

        CompletionTrend.Clear();
        foreach (var s in _analyticsService.GetCompletionTrend(30, CurrentBoardId))
            CompletionTrend.Add(s);

        OwnerWorkload.Clear();
        foreach (var s in _analyticsService.GetOwnerWorkload(CurrentBoardId))
            OwnerWorkload.Add(s);

        Q2Percentage = _analyticsService.GetQ2Percentage(CurrentBoardId);
    }

    // ====================== TAGS MANAGEMENT ======================

    [RelayCommand]
    private void AddTag()
    {
        var tag = _tagService.AddTag($"标签{AllTags.Count + 1}");
        LoadTags();
    }

    [RelayCommand]
    private void DeleteTag(Tag? tag)
    {
        if (tag == null) return;
        _tagService.DeleteTag(tag.Id);
        LoadTags();
        LoadTasks();
    }

    // ====================== TEMPLATES ======================

    [RelayCommand]
    private void DeleteTemplate(TaskTemplate? template)
    {
        if (template == null) return;
        _templateService.DeleteTemplate(template.Id);
        LoadTemplates();
    }
}
