using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.MHolidayPages;

/// <summary>
/// 入力モデル
/// </summary>
public sealed class InputModel : BaseInputModel
{
    /// <summary>
    /// 日付
    /// </summary>
    [Required]
    [Display(Name = "日付")]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}