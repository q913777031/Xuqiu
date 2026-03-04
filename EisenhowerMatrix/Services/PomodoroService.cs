using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Services;

public class PomodoroService
{
    private readonly IFreeSql _freeSql;

    public PomodoroService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public void AddRecord(PomodoroRecord record)
    {
        _freeSql.Insert(record).ExecuteAffrows();

        // Update task totals
        if (record.Completed == 1)
        {
            _freeSql.Update<TaskItem>()
                .Set(t => t.TotalFocusTime + record.Duration)
                .Set(t => t.PomodoroCount + 1)
                .Where(t => t.Id == record.TaskId)
                .ExecuteAffrows();
        }
    }

    public List<PomodoroRecord> GetRecordsForTask(int taskId)
    {
        return _freeSql.Select<PomodoroRecord>()
            .Where(r => r.TaskId == taskId)
            .OrderByDescending(r => r.StartTime)
            .ToList();
    }

    public List<PomodoroRecord> GetRecordsByDateRange(DateTime start, DateTime end)
    {
        return _freeSql.Select<PomodoroRecord>()
            .Where(r => r.StartTime >= start && r.StartTime <= end && r.Completed == 1)
            .OrderBy(r => r.StartTime)
            .ToList();
    }

    public int GetTodayPomodoroCount()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        return (int)_freeSql.Select<PomodoroRecord>()
            .Where(r => r.StartTime >= today && r.StartTime < tomorrow && r.Completed == 1)
            .Count();
    }

    public int GetTodayFocusMinutes()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var totalSeconds = _freeSql.Select<PomodoroRecord>()
            .Where(r => r.StartTime >= today && r.StartTime < tomorrow && r.Completed == 1)
            .Sum(r => r.Duration);
        return (int)(totalSeconds / 60);
    }
}
