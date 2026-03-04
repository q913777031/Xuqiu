using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Services;

public class TaskService
{
    private readonly IFreeSql _freeSql;

    public TaskService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public List<TaskItem> GetAllTasks()
    {
        return _freeSql.Select<TaskItem>()
            .OrderBy(t => t.Quadrant)
            .OrderBy(t => t.SortOrder)
            .ToList();
    }

    public List<TaskItem> GetTasksByQuadrant(QuadrantType quadrant)
    {
        return _freeSql.Select<TaskItem>()
            .Where(t => t.Quadrant == quadrant)
            .OrderBy(t => t.SortOrder)
            .ToList();
    }

    public TaskItem AddTask(TaskItem task)
    {
        var maxSort = _freeSql.Select<TaskItem>()
            .Where(t => t.Quadrant == task.Quadrant)
            .Max(t => t.SortOrder);

        task.SortOrder = maxSort + 1;
        task.CreatedAt = DateTime.Now;
        task.UpdatedAt = DateTime.Now;

        _freeSql.Insert(task).ExecuteAffrows();

        // Retrieve with generated Id
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
        _freeSql.Delete<TaskItem>()
            .Where(t => t.Id == id)
            .ExecuteAffrows();
    }

    public void UpdateSortOrders(List<TaskItem> tasks)
    {
        foreach (var task in tasks)
        {
            task.UpdatedAt = DateTime.Now;
        }

        _freeSql.Update<TaskItem>()
            .SetSource(tasks)
            .ExecuteAffrows();
    }
}
