using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkOps.Models;

/// <summary>
/// 工程
/// </summary>
public class MPhase : BaseEntity
{
    /// <summary>
    /// プロジェクトID
    /// </summary>
    [ForeignKey(nameof(MPhase))]
    public int MProjectId { get; set; }

    /// <summary>
    /// プロジェクト
    /// </summary>
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public MProject MProject { get; set; } = null!;

    /// <summary>
    /// 予定リスト
    /// </summary>
    public ICollection<TPlan> TPlans { get; set; } = [];

    /// <summary>
    /// 実績リスト
    /// </summary>
    public ICollection<TActual> TActuals { get; set; } = [];

    /// <summary>
    /// 工程
    /// </summary>
    public MPhase() : base()
    {
    }

    /// <summary>
    /// 工程
    /// </summary>
    /// <param name="model"></param>
    public MPhase(BaseInputModel model) : base(model)
    {
    }
}
