using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.MWorkTimePages;

/// <summary>
/// 入力モデル
/// </summary>
public sealed class InputModel : BaseInputModel
{
    /// <summary>
    /// 開始時間
    /// </summary>
    [Display(Name = "開始時間")]
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// 終了時間
    /// </summary>
    [Display(Name = "終了時間")]
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// 休憩開始時間
    /// </summary>
    [Display(Name = "休憩開始時間")]
    public TimeOnly BreakStartTime { get; set; }

    /// <summary>
    /// 休憩終了時間
    /// </summary>
    [Display(Name = "休憩終了時間")]
    public TimeOnly BreakEndTime { get; set; }

    /// <summary>
    /// 勤務時間
    /// </summary>
    public TimeSpan? WorkedDuration { get; set; }

    /// <summary>
    /// 勤務時間文字列
    /// </summary>
    [Display(Name = "勤務時間")]
    public string WorkedDurationString
    {
        get
        {
            return WorkedDuration?.ToString(@"hh\:mm") ?? string.Empty;
        }
        set
        {
            WorkedDuration = TimeSpan.TryParse(value, out var ts) ? ts : null;
        }
    }

    /// <summary>
    /// 勤務時間文字列
    /// </summary>
    [Display(Name = "勤務時間")]
    public TimeOnly WorkedDurationTime { get; set; }
}