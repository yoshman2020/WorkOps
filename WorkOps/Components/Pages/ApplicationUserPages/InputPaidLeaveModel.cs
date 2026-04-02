using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.ApplicationUserPages;

/// <summary>
/// 有給休暇
/// </summary>
public class InputPaidLeaveModel : BaseInputModel
{
    /// <summary>
    /// 担当者ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 付与日数
    /// </summary>
    [Display(Name = "有給休暇日数")]
    public int GrantedDays { get; set; }

    /// <summary>
    /// 付与日
    /// </summary>
    [Display(Name = "付与日")]
    public DateOnly GrantedDate { get; set; }

    /// <summary>
    /// 有効期限
    /// </summary>
    [Display(Name = "有効期限")]
    public DateOnly ExpiredDate { get; set; } = DateOnly.MaxValue;
}
