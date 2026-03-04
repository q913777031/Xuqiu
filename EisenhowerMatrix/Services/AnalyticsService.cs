using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Services;

public class QuadrantStat
{
    public QuadrantType Quadrant { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class DailyCompletionStat
{
    public DateTime Date { get; set; }
    public int CompletedCount { get; set; }
}

public class OwnerWorkloadStat
{
    public string Owner { get; set; } = "";
    public int TaskCount { get; set; }
    public int CompletedCount { get; set; }
}

public class AnalyticsService
{
    private readonly IFreeSql _freeSql;

    public AnalyticsService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public List<QuadrantStat> GetQuadrantDistribution(int boardId = 1)
    {
        var tasks = _freeSql.Select<TaskItem>()
            .Where(t => t.BoardId == boardId && t.ParentId == null && t.IsArchived == 0)
            .ToList();

        var total = tasks.Count;
        if (total == 0) return new List<QuadrantStat>();

        return Enum.GetValues<QuadrantType>().Select(q => new QuadrantStat
        {
            Quadrant = q,
            Count = tasks.Count(t => t.Quadrant == q),
            Percentage = Math.Round(tasks.Count(t => t.Quadrant == q) * 100.0 / total, 1)
        }).ToList();
    }

    public List<DailyCompletionStat> GetCompletionTrend(int days = 30, int boardId = 1)
    {
        var startDate = DateTime.Today.AddDays(-days + 1);
        var tasks = _freeSql.Select<TaskItem>()
            .Where(t => t.BoardId == boardId && t.ParentId == null
                        && t.Status == TaskItemStatus.Completed
                        && t.UpdatedAt >= startDate)
            .ToList();

        var result = new List<DailyCompletionStat>();
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            result.Add(new DailyCompletionStat
            {
                Date = date,
                CompletedCount = tasks.Count(t => t.UpdatedAt.Date == date)
            });
        }
        return result;
    }

    public List<OwnerWorkloadStat> GetOwnerWorkload(int boardId = 1)
    {
        var tasks = _freeSql.Select<TaskItem>()
            .Where(t => t.BoardId == boardId && t.ParentId == null && t.IsArchived == 0
                        && t.Owner != null && t.Owner != "")
            .ToList();

        return tasks.GroupBy(t => t.Owner!)
            .Select(g => new OwnerWorkloadStat
            {
                Owner = g.Key,
                TaskCount = g.Count(),
                CompletedCount = g.Count(t => t.Status == TaskItemStatus.Completed)
            })
            .OrderByDescending(s => s.TaskCount)
            .ToList();
    }

    public double GetQ2Percentage(int boardId = 1)
    {
        var total = _freeSql.Select<TaskItem>()
            .Where(t => t.BoardId == boardId && t.ParentId == null && t.IsArchived == 0)
            .Count();

        if (total == 0) return 0;

        var q2Count = _freeSql.Select<TaskItem>()
            .Where(t => t.BoardId == boardId && t.ParentId == null && t.IsArchived == 0
                        && t.Quadrant == QuadrantType.Q2)
            .Count();

        return Math.Round(q2Count * 100.0 / total, 1);
    }

    public int GetTotalCompletedCount(int days = 7, int boardId = 1)
    {
        var startDate = DateTime.Today.AddDays(-days);
        return (int)_freeSql.Select<TaskItem>()
            .Where(t => t.BoardId == boardId && t.Status == TaskItemStatus.Completed
                        && t.UpdatedAt >= startDate)
            .Count();
    }
}
