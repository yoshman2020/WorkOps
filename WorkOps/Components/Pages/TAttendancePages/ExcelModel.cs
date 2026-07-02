using WorkOps.Models.Attributes;

namespace WorkOps.Components.Pages.TAttendancePages;

/// <summary>
/// Excelモデル
/// </summary>
public sealed class ExcelModel
{
    /// <summary>
    /// 日
    /// </summary>
    [ExcelColumn(Header = "日", Width = 4.75)]
    public int Day { get; set; }

    /// <summary>
    /// 曜日
    /// </summary>
    [ExcelColumn(Header = "曜日", Width = 6.5)]
    public string DayOfWeek { get; set; } = string.Empty;

    /// <summary>
    /// 祝祭日
    /// </summary>
    [ExcelColumn(Header = "祝祭日", Width = 11.5)]
    public string HolidayName { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間
    /// </summary>
    [ExcelColumn(Header = "開始時間", Width = 9.0)]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 終了時間
    /// </summary>
    [ExcelColumn(Header = "終了時間", Width = 9.0)]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 有給時間の時刻表示
    /// </summary>
    [ExcelColumn(Header = "有給時間", Width = 9.0)]
    public TimeSpan? PaidLeaveDuration { get; set; }

    /// <summary>
    /// 勤務時間の時刻表示
    /// </summary>
    [ExcelColumn(Header = "勤務時間", Width = 9.0)]
    public TimeSpan? WorkedDuration { get; set; }

    /// <summary>
    /// 時間外の時刻表示
    /// </summary>
    [ExcelColumn(Header = "時間外", Width = 9.0)]
    public TimeSpan? OvertimeDuration { get; set; }

    /// <summary>
    /// 備考
    /// </summary>
    [ExcelColumn(Header = "備考", Width = 21.38)]
    public string? Remarks { get; set; }

    /// <summary>
    /// 作業内容午前
    /// </summary>
    [ExcelColumn(Header = "AM", Width = 45.13)]
    public string? WorkDetailAm { get; set; }

    /// <summary>
    /// 作業内容午後
    /// </summary>
    [ExcelColumn(Header = "PM", Width = 45.13)]
    public string? WorkDetailPm { get; set; }
}