using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Services;

public class TemplateService
{
    private readonly IFreeSql _freeSql;

    public TemplateService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public List<TaskTemplate> GetAllTemplates()
    {
        return _freeSql.Select<TaskTemplate>().OrderBy(t => t.Name).ToList();
    }

    public TaskTemplate AddTemplate(TaskTemplate template)
    {
        _freeSql.Insert(template).ExecuteAffrows();
        return _freeSql.Select<TaskTemplate>().OrderByDescending(t => t.Id).First();
    }

    public void UpdateTemplate(TaskTemplate template)
    {
        _freeSql.Update<TaskTemplate>().SetSource(template).ExecuteAffrows();
    }

    public void DeleteTemplate(int id)
    {
        _freeSql.Delete<TaskTemplate>().Where(t => t.Id == id).ExecuteAffrows();
    }
}
