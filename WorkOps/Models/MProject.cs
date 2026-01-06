using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkOps.Models;

/// <summary>
/// プロジェクト
/// </summary>
public class MProject : BaseEntity
{
    /// <summary>
    /// 顧客ID
    /// </summary>
    [ForeignKey(nameof(MCustomer))]
    public int MCustomerId { get; set; }

    /// <summary>
    /// 顧客
    /// </summary>
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public MCustomer MCustomer { get; set; } = null!;

    /// <summary>
    /// 工程リスト
    /// </summary>
    public ICollection<MPhase> MPhases { get; set; } = [];

    /// <summary>
    /// 週報詳細リスト
    /// </summary>
    public ICollection<TReportDetail> TReportDetails { get; set; } = [];

    /// <summary>
    /// プロジェクト
    /// </summary>
    public MProject() : base()
    {
    }

    /// <summary>
    /// プロジェクト
    /// </summary>
    /// <param name="model"></param>
    public MProject(BaseInputModel model) : base(model)
    {
    }
}
