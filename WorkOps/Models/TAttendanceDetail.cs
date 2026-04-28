using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkOps.Models;

/// <summary>
/// 出退勤詳細
/// </summary>
public class TAttendanceDetail : BaseEntity
{
    /// <summary>
    /// 出退勤ID
    /// </summary>
    [ForeignKey(nameof(TAttendance))]
    public int TAttendanceId { get; set; }

    /// <summary>
    /// 出退勤
    /// </summary>
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public TAttendance? TAttendance { get; set; }

    /// <summary>
    /// 作業内容午前
    /// </summary>
    public string? WorkDetailAm { get; set; }

    /// <summary>
    /// 作業内容午後
    /// </summary>
    public string? WorkDetailPm { get; set; }
}
