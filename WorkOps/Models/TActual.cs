using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using WorkOps.Data;

namespace WorkOps.Models;

/// <summary>
/// 実績
/// </summary>
public class TActual : BaseEntity
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
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 担当者
    /// </summary>
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// 開始日時
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 終了日時
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 作業工数
    /// </summary>
    public double ManHour { get; set; }

    /// <summary>
    /// 作業内容
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 進捗率
    /// </summary>
    public int? ProgressRate { get; set; }

    /// <summary>
    /// 実績
    /// </summary>
    public TActual() : base()
    {
    }

    /// <summary>
    /// 実績
    /// </summary>
    /// <param name="model"></param>
    public TActual(BaseInputModel model) : base(model)
    {
    }
}
