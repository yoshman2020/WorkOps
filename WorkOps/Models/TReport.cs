namespace WorkOps.Models;

/// <summary>
/// 週報
/// </summary>
public class TReport : BaseEntity
{
    /// <summary>
    /// 担当者ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 開始日
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 週報詳細リスト
    /// </summary>
    public ICollection<TReportDetail> TReportDetails { get; set; } = [];

    /// <summary>
    /// 週報
    /// </summary>
    public TReport() : base()
    {
    }

    /// <summary>
    /// 週報
    /// </summary>
    /// <param name="model"></param>
    public TReport(BaseInputModel model) : base(model)
    {
    }
}
