using FreeSql.DataAnnotations;

namespace EisenhowerMatrix.Models;

[Table(Name = "AppSettings")]
public class AppSetting
{
    [Column(IsPrimary = true, StringLength = 100)]
    public string Key { get; set; } = string.Empty;

    [Column(IsNullable = false)]
    public string Value { get; set; } = string.Empty;
}
