using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Services;

public class TagService
{
    private readonly IFreeSql _freeSql;

    public TagService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public List<Tag> GetAllTags()
    {
        return _freeSql.Select<Tag>().OrderBy(t => t.Name).ToList();
    }

    public Tag AddTag(string name, string color = "#3B82F6")
    {
        var existing = _freeSql.Select<Tag>().Where(t => t.Name == name).First();
        if (existing != null) return existing;

        var tag = new Tag { Name = name, Color = color };
        _freeSql.Insert(tag).ExecuteAffrows();
        return _freeSql.Select<Tag>().Where(t => t.Name == name).First();
    }

    public void UpdateTag(Tag tag)
    {
        _freeSql.Update<Tag>().SetSource(tag).ExecuteAffrows();
    }

    public void DeleteTag(int id)
    {
        _freeSql.Delete<TaskTag>().Where(tt => tt.TagId == id).ExecuteAffrows();
        _freeSql.Delete<Tag>().Where(t => t.Id == id).ExecuteAffrows();
    }

    public List<Tag> GetTagsForTask(int taskId)
    {
        var tagIds = _freeSql.Select<TaskTag>()
            .Where(tt => tt.TaskId == taskId)
            .ToList(tt => tt.TagId);

        if (tagIds.Count == 0) return new List<Tag>();

        return _freeSql.Select<Tag>().Where(t => tagIds.Contains(t.Id)).ToList();
    }

    public void SetTaskTags(int taskId, List<int> tagIds)
    {
        _freeSql.Delete<TaskTag>().Where(tt => tt.TaskId == taskId).ExecuteAffrows();

        if (tagIds.Count == 0) return;

        var taskTags = tagIds.Select(tid => new TaskTag { TaskId = taskId, TagId = tid }).ToList();
        _freeSql.Insert(taskTags).ExecuteAffrows();
    }
}
