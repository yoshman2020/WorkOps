using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkOps.Models;

/// <summary>
/// 週報詳細
/// </summary>
public class TReportDetail : BaseEntity
{
    /// <summary>
    /// 週報ID
    /// </summary>
    [ForeignKey(nameof(TReport))]
    public int TReportId { get; set; }

    /// <summary>
    /// 週報
    /// </summary>
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public TReport TReport { get; set; } = null!;

    /// <summary>
    /// プロジェクトID
    /// </summary>
    [ForeignKey(nameof(MProject))]
    public int? MProjectId { get; set; }

    /// <summary>
    /// プロジェクト
    /// </summary>
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public MProject? MProject { get; set; }

    /// <summary>
    /// 作業内容
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 課題・問題
    /// </summary>
    public string Problem { get; set; } = string.Empty;

    /// <summary>
    /// 今後の予定
    /// </summary>
    public string Schedule { get; set; } = string.Empty;

    /// <summary>
    /// 週報詳細
    /// </summary>
    public TReportDetail() : base()
    {
    }

    /// <summary>
    /// 週報詳細
    /// </summary>
    /// <param name="model"></param>
    public TReportDetail(BaseInputModel model) : base(model)
    {
    }

}
