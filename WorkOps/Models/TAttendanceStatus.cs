using System.ComponentModel.DataAnnotations.Schema;
using WorkOps.Data;

namespace WorkOps.Models;

/// <summary>
/// 出退勤ステータス
/// </summary>
public class TAttendanceStatus : BaseEntity
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
    /// 年
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 月
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// 承認ステータスID
    /// </summary>
    [ForeignKey(nameof(MApprovalStatus))]
    public int? MApprovalStatusId { get; set; }

    /// <summary>
    /// 承認ステータス
    /// </summary>
    public MApprovalStatus? MApprovalStatus { get; set; }
}
