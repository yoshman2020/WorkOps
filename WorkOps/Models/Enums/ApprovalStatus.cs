using System.ComponentModel.DataAnnotations;

namespace WorkOps.Models.Enums;

/// <summary>
/// 承認ステータス
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// 未提出
    /// </summary>
    [Display(Name = "未提出", ShortName = "未提出")]
    NotSubmitted = 0,

    /// <summary>
    /// 提出済（管理者確認中）
    /// </summary>
    [Display(Name = "提出済\n（管理者確認中）", ShortName = "提出済")]
    SubmittedPendingManager = 1,

    /// <summary>
    /// 管理者確認済（担当者確認中）
    /// </summary>
    [Display(Name = "管理者確認済\n（担当者確認中）", ShortName = "管理者確認済")]
    UnderReviewByStaff = 2,

    /// <summary>
    /// 担当者確認済（管理者承認待）
    /// </summary>
    [Display(Name = "担当者確認済\n（管理者承認待）", ShortName = "担当者確認済")]
    ReviewedPendingApproval = 3,

    /// <summary>
    /// 承認済
    /// </summary>
    [Display(Name = "承認済", ShortName = "承認済")]
    Approved = 4
}
