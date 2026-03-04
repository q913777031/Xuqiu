using System.IO;
using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Data;

public static class AppDbContext
{
    private static IFreeSql? _freeSql;

    public static IFreeSql Instance => _freeSql ?? throw new InvalidOperationException("Database not initialized.");

    public static IFreeSql Initialize()
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eisenhower.db");

        _freeSql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.Sqlite, $"Data Source={dbPath};Pooling=true;")
            .UseAutoSyncStructure(true)
            .Build();

        // CodeFirst sync all tables
        _freeSql.CodeFirst.SyncStructure<Board>();
        _freeSql.CodeFirst.SyncStructure<TaskItem>();
        _freeSql.CodeFirst.SyncStructure<Tag>();
        _freeSql.CodeFirst.SyncStructure<TaskTag>();
        _freeSql.CodeFirst.SyncStructure<PomodoroRecord>();
        _freeSql.CodeFirst.SyncStructure<TaskTemplate>();
        _freeSql.CodeFirst.SyncStructure<QuadrantConfig>();
        _freeSql.CodeFirst.SyncStructure<AppSetting>();
        _freeSql.CodeFirst.SyncStructure<OperationLog>();

        // Seed default board if none exists
        var boardCount = _freeSql.Select<Board>().Count();
        if (boardCount == 0)
        {
            _freeSql.Insert(new Board
            {
                Name = "我的任务",
                Description = "默认看板",
                Color = "#3B82F6",
                SortOrder = 0,
                CreatedAt = DateTime.Now
            }).ExecuteAffrows();
        }

        return _freeSql;
    }

    public static void Dispose()
    {
        _freeSql?.Dispose();
        _freeSql = null;
    }
}
