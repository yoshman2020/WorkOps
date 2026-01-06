using System.ComponentModel.DataAnnotations;
using WorkOps.Data;

namespace WorkOps.Models;

/// <summary>
/// 勤務時間
/// </summary>
public class MWorkTime : BaseEntity
{
    /// <summary>
    /// 開始時間
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// 終了時間
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// 休憩開始時間
    /// </summary>
    public TimeOnly BreakStartTime { get; set; }

    /// <summary>
    /// 休憩終了時間
    /// </summary>
    public TimeOnly BreakEndTime { get; set; }

    /// <summary>
    /// 勤務時間
    /// </summary>
    public TimeSpan? WorkedDuration { get; set; }

    /// <summary>
    /// 担当者リスト
    /// </summary>
    public ICollection<ApplicationUser> ApplicationUsers { get; set; } = [];

    /// <summary>
    /// 勤務時間
    /// </summary>
    public MWorkTime() : base()
    {
    }

    /// <summary>
    /// 勤務時間
    /// </summary>
    /// <param name="model"></param>
    public MWorkTime(BaseInputModel model) : base(model)
    {
    }
}
