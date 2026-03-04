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

        // Trigger CodeFirst sync
        _freeSql.CodeFirst.SyncStructure<TaskItem>();

        return _freeSql;
    }

    public static void Dispose()
    {
        _freeSql?.Dispose();
        _freeSql = null;
    }
}
