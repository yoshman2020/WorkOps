namespace WorkOps.Models.Attributes;

/// <summary>
/// Excel列属性
/// </summary>
/// <param name="width"></param>
[AttributeUsage(AttributeTargets.Property)]
public class ExcelColumnAttribute(string? header = null, double width = 9)
    : Attribute
{
    /// <summary>
    /// ヘッダー
    /// </summary>
    public string? Header { get; set; } = header;

    /// <summary>
    /// 列幅
    /// </summary>
    public double Width { get; set; } = width;
}
