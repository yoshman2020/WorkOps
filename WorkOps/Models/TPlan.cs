using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using WorkOps.Data;

namespace WorkOps.Models;

/// <summary>
/// 予定
/// </summary>
public class TPlan : BaseEntity
{
    /// <summary>
    /// 工程ID
    /// </summary>
    [ForeignKey(nameof(MPhase))]
    public int MPhaseId { get; set; }

    /// <summary>
    /// 工程
    /// </summary>
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public MPhase MPhase { get; set; } = null!;

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
    /// 開始日時
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 終了時間
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 作業工数
    /// </summary>
    public double ManHour { get; set; }

    /// <summary>
    /// 予定
    /// </summary>
    public TPlan() : base()
    {
    }

    /// <summary>
    /// 予定
    /// </summary>
    /// <param name="model"></param>
    public TPlan(BaseInputModel model) : base(model)
    {
    }
}
