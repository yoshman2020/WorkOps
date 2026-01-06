using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.MPhasePages;

/// <summary>
/// 入力モデル
/// </summary>
public sealed class InputModel : BaseInputModel
{
    /// <summary>
    /// プロジェクトID
    /// </summary>
    [Required]
    [Display(Name = "プロジェクト")]
    public int? MProjectId { get; set; }

    /// <summary>
    /// プロジェクト
    /// </summary>
    [Display(Name = "プロジェクト")]

    public string? ProjectName { get; set; }

    /// <summary>
    /// 顧客
    /// </summary>
    [Display(Name = "顧客")]
    public string? CustomerName { get; set; }
}