using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using WorkOps.Data;

namespace WorkOps.Models;

/// <summary>
/// 週報
/// </summary>
public class TReport : BaseEntity
{
    /// <summary>
    /// 担当者ID
    /// </summary>
    [ForeignKey(nameof(ApplicationUser))]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 担当者
    /// </summary>
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// 開始日
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 週報詳細リスト
    /// </summary>
    public ICollection<TReportDetail> TReportDetails { get; set; } = [];

    /// <summary>
    /// 承認ステータスID
    /// </summary>
    [ForeignKey(nameof(MApprovalStatus))]
    public int? MApprovalStatusId { get; set; }

    /// <summary>
    /// 承認ステータス
    /// </summary>
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public MApprovalStatus MApprovalStatus { get; set; } = null!;

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
