using System.IO;
using System.Text;
using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Services;

public class CsvService
{
    private readonly IFreeSql _freeSql;

    public CsvService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public string ExportToCsv(List<TaskItem> tasks, TagService tagService)
    {
        var sb = new StringBuilder();
        sb.AppendLine("标题,象限,负责人,预估工期,状态,阻塞点,截止日期,标签,创建时间");

        foreach (var t in tasks)
        {
            var tags = tagService.GetTagsForTask(t.Id);
            var tagNames = string.Join(";", tags.Select(tg => tg.Name));
            var quadrant = t.Quadrant switch
            {
                QuadrantType.Q1 => "Q1",
                QuadrantType.Q2 => "Q2",
                QuadrantType.Q3 => "Q3",
                QuadrantType.Q4 => "Q4",
                _ => "Q1"
            };
            var status = t.Status switch
            {
                TaskItemStatus.NotStarted => "未开始",
                TaskItemStatus.InProgress => "进行中",
                TaskItemStatus.Completed => "已完成",
                TaskItemStatus.Blocked => "阻塞",
                _ => "未开始"
            };
            var dueDate = t.DueDate?.ToString("yyyy-MM-dd") ?? "";

            sb.AppendLine($"\"{Escape(t.Title)}\",{quadrant},\"{Escape(t.Owner)}\",\"{Escape(t.Estimate)}\",{status},\"{Escape(t.Blocker)}\",{dueDate},\"{Escape(tagNames)}\",{t.CreatedAt:yyyy-MM-dd}");
        }

        return sb.ToString();
    }

    public List<TaskItem> ParseCsv(string csvContent, int boardId = 1)
    {
        var tasks = new List<TaskItem>();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2) return tasks;

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i].Trim());
            if (fields.Count == 0 || string.IsNullOrWhiteSpace(fields[0])) continue;

            var task = new TaskItem
            {
                Title = fields[0],
                BoardId = boardId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            if (fields.Count > 1) task.Quadrant = ParseQuadrant(fields[1]);
            if (fields.Count > 2) task.Owner = string.IsNullOrWhiteSpace(fields[2]) ? null : fields[2];
            if (fields.Count > 3) task.Estimate = string.IsNullOrWhiteSpace(fields[3]) ? null : fields[3];
            if (fields.Count > 4) task.Status = ParseStatus(fields[4]);
            if (fields.Count > 5) task.Blocker = string.IsNullOrWhiteSpace(fields[5]) ? null : fields[5];
            if (fields.Count > 6 && DateTime.TryParse(fields[6], out var dueDate)) task.DueDate = dueDate;

            tasks.Add(task);
        }

        return tasks;
    }

    private static string Escape(string? value)
    {
        if (value == null) return "";
        return value.Replace("\"", "\"\"");
    }

    private static QuadrantType ParseQuadrant(string value)
    {
        return value.Trim().ToUpper() switch
        {
            "Q1" => QuadrantType.Q1,
            "Q2" => QuadrantType.Q2,
            "Q3" => QuadrantType.Q3,
            "Q4" => QuadrantType.Q4,
            _ => QuadrantType.Q1
        };
    }

    private static TaskItemStatus ParseStatus(string value)
    {
        return value.Trim() switch
        {
            "进行中" or "InProgress" => TaskItemStatus.InProgress,
            "已完成" or "Completed" => TaskItemStatus.Completed,
            "阻塞" or "Blocked" => TaskItemStatus.Blocked,
            _ => TaskItemStatus.NotStarted
        };
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields;
    }
}
