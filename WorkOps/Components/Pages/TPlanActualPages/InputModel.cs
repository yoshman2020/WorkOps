using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.TPlanActualPages;

/// <summary>
/// 入力モデル
/// </summary>
public sealed class InputModel : BaseInputModel
{
    /// <summary>
    /// 行クラス
    /// </summary>
    public string RowClass { get; set; }　= string.Empty;

    /// <summary>
    /// 実績
    /// </summary>
    public bool IsActual { get; set; }

    /// <summary>
    /// 予定・実績
    /// </summary>
    [Display(Name = "")]
    public string TypeName { get =>
            RowClass switch { "actual" => "実績", "plan" => "予定", _ => "" }; }

    /// <summary>
    /// 日付ごと予定・実績IDと表示文字（―――>）とツールチップ
    /// </summary>
    public Dictionary<DateTime,
        (int Id, string DisplayText, string Tooltip)> Cells = default!;

    /// <summary>
    /// 顧客ID
    /// </summary>
    public int? MCustomerId { get; set; }

    /// <summary>
    /// 顧客名
    /// </summary>
    [Display(Name = "顧客")]
    public string? CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// プロジェクトID
    /// </summary>
    public int? MProjectId { get; set; }

    /// <summary>
    /// プロジェクト名
    /// </summary>
    [Display(Name = "プロジェクト")]
    public string? ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// 工程ID
    /// </summary>
    [Required]
    [Display(Name = "工程")]
    public int? MPhaseId { get; set; }

    /// <summary>
    /// 工程
    /// </summary>
    [Display(Name = "業務名、作業内容")]
    public string? PhaseName { get; set; } = string.Empty;

    /// <summary>
    /// 工程別期間内作業工数
    /// </summary>
    [Display(Name = "工程別期間内作業工数")]
    public double ManHour { get; set; }

    /// <summary>
    /// 工程別期間内作業工数文字列
    /// </summary>
    [Display(Name = "(期間内)")]
    public string ManHourStr
        => ManHour == 0 ? "" : $"({ManHour:#.#})";

    /// <summary>
    /// 工程別累計作業工数
    /// </summary>
    [Display(Name = "作業工数")]
    public double PhaseTotalManHour { get; set; }

    /// <summary>
    /// 進捗率文字列
    /// </summary>
    [Display(Name = "進捗率")]
    public string? ProgressRateString { get; set; }

    /// <summary>
    /// 終了日
    /// </summary>
    [Display(Name = "終了")]
    public DateTime? EndDate { get; set; }
}