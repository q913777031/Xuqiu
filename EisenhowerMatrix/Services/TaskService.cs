using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Services;

public class TaskService
{
    private readonly IFreeSql _freeSql;

    public TaskService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public List<TaskItem> GetAllTasks(int boardId = 1, bool includeArchived = false)
    {
        var query = _freeSql.Select<TaskItem>()
            .Where(t => t.BoardId == boardId && t.ParentId == null);

        if (!includeArchived)
            query = query.Where(t => t.IsArchived == 0);

        return query
            .OrderBy(t => t.Quadrant)
            .OrderBy(t => t.SortOrder)
            .ToList();
    }

    public List<TaskItem> GetTasksByQuadrant(QuadrantType quadrant, int boardId = 1)
    {
        return _freeSql.Select<TaskItem>()
            .Where(t => t.Quadrant == quadrant && t.BoardId == boardId && t.ParentId == null && t.IsArchived == 0)
            .OrderBy(t => t.SortOrder)
            .ToList();
    }

    public List<TaskItem> GetSubtasks(int parentId)
    {
        return _freeSql.Select<TaskItem>()
            .Where(t => t.ParentId == parentId)
            .OrderBy(t => t.SortOrder)
            .ToList();
    }

    public int GetSubtaskCount(int parentId)
    {
        return (int)_freeSql.Select<TaskItem>().Where(t => t.ParentId == parentId).Count();
    }

    public int GetCompletedSubtaskCount(int parentId)
    {
        return (int)_freeSql.Select<TaskItem>()
            .Where(t => t.ParentId == parentId && t.Status == TaskItemStatus.Completed)
            .Count();
    }

    public TaskItem AddTask(TaskItem task)
    {
        var maxSort = _freeSql.Select<TaskItem>()
            .Where(t => t.Quadrant == task.Quadrant && t.BoardId == task.BoardId && t.ParentId == task.ParentId)
            .Max(t => t.SortOrder);

        task.SortOrder = maxSort + 1;
        task.CreatedAt = DateTime.Now;
        task.UpdatedAt = DateTime.Now;

        _freeSql.Insert(task).ExecuteAffrows();

        return _freeSql.Select<TaskItem>()
            .Where(t => t.Title == task.Title && t.CreatedAt == task.CreatedAt)
            .OrderByDescending(t => t.Id)
            .First();
    }

    public void UpdateTask(TaskItem task)
    {
        task.UpdatedAt = DateTime.Now;
        _freeSql.Update<TaskItem>()
            .SetSource(task)
            .ExecuteAffrows();
    }

    public void DeleteTask(int id)
    {
        // Delete subtasks first
        _freeSql.Delete<TaskItem>().Where(t => t.ParentId == id).ExecuteAffrows();
        // Delete tag associations
        _freeSql.Delete<TaskTag>().Where(tt => tt.TaskId == id).ExecuteAffrows();
        // Delete pomodoro records
        _freeSql.Delete<PomodoroRecord>().Where(p => p.TaskId == id).ExecuteAffrows();
        // Delete task
        _freeSql.Delete<TaskItem>().Where(t => t.Id == id).ExecuteAffrows();
    }

    public TaskItem? GetTaskById(int id)
    {
        return _freeSql.Select<TaskItem>().Where(t => t.Id == id).First();
    }

    public void UpdateSortOrders(List<TaskItem> tasks)
    {
        foreach (var task in tasks)
            task.UpdatedAt = DateTime.Now;

        _freeSql.Update<TaskItem>().SetSource(tasks).ExecuteAffrows();
    }

    public List<TaskItem> SearchTasks(string keyword, int boardId = 1)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return GetAllTasks(boardId);

        return _freeSql.Select<TaskItem>()
            .Where(t => t.BoardId == boardId && t.ParentId == null && t.IsArchived == 0)
            .Where(t => t.Title.Contains(keyword) ||
                        (t.Owner != null && t.Owner.Contains(keyword)) ||
                        (t.Blocker != null && t.Blocker.Contains(keyword)))
            .OrderBy(t => t.Quadrant)
            .OrderBy(t => t.SortOrder)
            .ToList();
    }

    public List<TaskItem> GetOverdueTasks(int boardId = 1)
    {
        return _freeSql.Select<TaskItem>()
            .Where(t => t.BoardId == boardId && t.ParentId == null && t.IsArchived == 0
                        && t.DueDate != null && t.DueDate < DateTime.Now
                        && t.Status != TaskItemStatus.Completed)
            .ToList();
    }
}
