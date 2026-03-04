using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Services;

public class SettingsService
{
    private readonly IFreeSql _freeSql;
    private readonly Dictionary<string, string> _cache = new();

    public SettingsService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
        LoadAll();
    }

    private void LoadAll()
    {
        var all = _freeSql.Select<AppSetting>().ToList();
        foreach (var s in all)
            _cache[s.Key] = s.Value;
    }

    public string Get(string key, string defaultValue = "")
    {
        return _cache.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        var val = Get(key);
        return int.TryParse(val, out var result) ? result : defaultValue;
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        var val = Get(key);
        return bool.TryParse(val, out var result) ? result : defaultValue;
    }

    public void Set(string key, string value)
    {
        _cache[key] = value;
        var existing = _freeSql.Select<AppSetting>().Where(s => s.Key == key).First();
        if (existing != null)
        {
            existing.Value = value;
            _freeSql.Update<AppSetting>().SetSource(existing).ExecuteAffrows();
        }
        else
        {
            _freeSql.Insert(new AppSetting { Key = key, Value = value }).ExecuteAffrows();
        }
    }

    public void Set(string key, int value) => Set(key, value.ToString());
    public void Set(string key, bool value) => Set(key, value.ToString());

    // Convenience properties
    public string Theme
    {
        get => Get("theme", "Light");
        set => Set("theme", value);
    }

    public int PomodoroWorkMinutes
    {
        get => GetInt("pomodoro.work", 25);
        set => Set("pomodoro.work", value);
    }

    public int PomodoroBreakMinutes
    {
        get => GetInt("pomodoro.break", 5);
        set => Set("pomodoro.break", value);
    }

    public int PomodoroLongBreakMinutes
    {
        get => GetInt("pomodoro.longBreak", 15);
        set => Set("pomodoro.longBreak", value);
    }

    public int PomodoroLongBreakInterval
    {
        get => GetInt("pomodoro.longBreakInterval", 4);
        set => Set("pomodoro.longBreakInterval", value);
    }

    public bool MinimizeToTray
    {
        get => GetBool("tray.minimizeToTray", true);
        set => Set("tray.minimizeToTray", value);
    }

    public int LastBoardId
    {
        get => GetInt("lastBoardId", 1);
        set => Set("lastBoardId", value);
    }
}
