using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.TAttendancePages;

/// <summary>
/// 入力モデル
/// </summary>
public sealed class InputModel : BaseInputModel
{
    /// <summary>
    /// 担当者ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 担当者名
    /// </summary>
    [Display(Name = "担当者")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 祝祭日
    /// </summary>
    [Display(Name = "祝祭日")]
    public string? HolidayName { get; set; } = null!;

    /// <summary>
    /// 日付
    /// </summary>
    [Display(Name = "日付")]
    public DateOnly Date { get; set; }

    /// <summary>
    /// ログイン日時
    /// </summary>
    [Display(Name = "ログイン")]
    public DateTime? LoginTime { get; set; }

    /// <summary>
    /// ログアウト日時
    /// </summary>
    [Display(Name = "ログアウト")]
    public DateTime? LogoutTime { get; set; }

    /// <summary>
    /// 開始時間
    /// </summary>
    [Display(Name = "開始時間")]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 終了時間
    /// </summary>
    [Display(Name = "終了時間")]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 有給時間
    /// </summary>
    public TimeSpan? PaidLeaveDuration { get; set; }

    /// <summary>
    /// 有給時間の時刻表示
    /// </summary>
    [Display(Name = "有給時間")]
    public DateTime? PaidLeaveDurationTime { get; set; }

    /// <summary>
    /// 有給時間の時刻表示文字列
    /// </summary>
    [Display(Name = "有給時間")]
    public string? PaidLeaveDurationString { get; set; }

    /// <summary>
    /// 勤務時間
    /// </summary>
    public TimeSpan? WorkedDuration { get; set; }

    /// <summary>
    /// 勤務時間の時刻表示
    /// </summary>
    [Display(Name = "勤務時間")]
    public DateTime? WorkedDurationTime { get; set; }

    /// <summary>
    /// 勤務時間の時刻表示文字列
    /// </summary>
    [Display(Name = "勤務時間")]
    public string? WorkedDurationString { get; set; }

    /// <summary>
    /// 時間外
    /// </summary>
    public TimeSpan? OvertimeDuration { get; set; }

    /// <summary>
    /// 時間外の時刻表示
    /// </summary>
    [Display(Name = "時間外")]
    public DateTime? OvertimeDurationTime { get; set; }

    /// <summary>
    /// 時間外の時刻表示文字列
    /// </summary>
    [Display(Name = "時間外")]
    public string? OvertimeDurationString { get; set; }

    /// <summary>
    /// 作業内容午前
    /// </summary>
    [Display(Name = "AM")]
    public string? WorkDetailAm { get; set; }

    /// <summary>
    /// 作業内容午後
    /// </summary>
    [Display(Name = "PM")]
    public string? WorkDetailPm { get; set; }

    /// <summary>
    /// 修正あり
    /// </summary>
    [Display(Name = "修正あり")]
    public bool IsModified { get; set; } = false;

    /// <summary>
    /// 修正承認済み
    /// </summary>
    [Display(Name = "修正承認済み")]
    public bool IsApproved { get; set; } = false;
}