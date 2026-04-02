using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.ApplicationUserPages;

/// <summary>
/// 入力モデル
/// </summary>
public class InputModel
{
    /// <summary>
    /// ユーザーID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    /// <summary>
    /// 役職
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 役職名
    /// </summary>
    [Display(Name = "役職")]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// 氏名
    /// </summary>
    [Display(Name = "ユーザー名")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 勤務時間ID
    /// </summary>
    public int? WorkTimeId { get; set; }

    /// <summary>
    /// 勤務時間文字列
    /// </summary>
    [Display(Name = "勤務時間")]
    public string WorkTimeString { get; set; } = string.Empty;

    /// <summary>
    /// 前日・先週の未入力を通知する
    /// </summary>
    [Display(Name = "前日・先週の未入力を通知する")]
    public bool IsPrevReminderEnabled { get; set; } = false;

    /// <summary>
    /// 前日・先週の未入力を通知する文字列
    /// </summary>
    public string IsPrevReminderEnabledStr => IsPrevReminderEnabled ? "有効" : "無効";

    /// <summary>
    /// 削除済み
    /// </summary>
    [Display(Name = "削除")]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 関連データも含めて完全に削除
    /// </summary>
    [Display(Name = "関連データも含めて完全に削除")]
    public bool IsForceDelete { get; set; } = false;

    /// <summary>
    /// 有給休暇
    /// </summary>
    public List<InputPaidLeaveModel> PaidLeaves { get; set; } = [];
}