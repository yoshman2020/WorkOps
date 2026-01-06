using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.TPlanPages;

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
    /// プロジェクト名
    /// </summary>
    [Display(Name = "プロジェクト")]
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// 工程ID
    /// </summary>
    [Required]
    [Display(Name = "工程")]
    public int? MPhaseId { get; set; }

    /// <summary>
    /// 工程
    /// </summary>
    [Display(Name = "工程")]
    public string? PhaseName { get; set; } = string.Empty;

    /// <summary>
    /// 開始日時
    /// </summary>
    [Display(Name = "開始日時")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 終了時間
    /// </summary>
    [Display(Name = "終了時間")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 作業工数
    /// </summary>
    [Display(Name = "作業工数")]
    public double ManHour { get; set; }
}