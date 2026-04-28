using System.ComponentModel.DataAnnotations.Schema;
using WorkOps.Data;

namespace WorkOps.Models;

/// <summary>
/// 出退勤
/// </summary>
public class TAttendance : BaseEntity
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
    /// 日付
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// ログイン日時
    /// </summary>
    public DateTime? LoginTime { get; set; }

    /// <summary>
    /// ログアウト日時
    /// </summary>
    public DateTime? LogoutTime { get; set; }

    /// <summary>
    /// 開始時間
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 終了時間
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 有給時間
    /// </summary>
    public TimeSpan? PaidLeaveDuration { get; set; }

    /// <summary>
    /// 勤務時間
    /// </summary>
    public TimeSpan? WorkedDuration { get; set; }

    /// <summary>
    /// 時間外
    /// </summary>
    public TimeSpan? OvertimeDuration { get; set; }

    /// <summary>
    /// 修正あり
    /// </summary>
    public bool IsModified { get; set; } = false;

    /// <summary>
    /// 修正承認済み
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// 出退勤詳細
    /// </summary>
    public TAttendanceDetail? TAttendanceDetail { get; set; }

    /// <summary>
    /// 出退勤
    /// </summary>
    public TAttendance() : base()
    {
    }

    /// <summary>
    /// 出退勤
    /// </summary>
    /// <param name="model"></param>
    public TAttendance(BaseInputModel model) : base(model)
    {
    }
}
