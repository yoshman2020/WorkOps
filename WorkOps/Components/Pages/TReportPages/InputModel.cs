using System.ComponentModel.DataAnnotations;
using WorkOps.Models;
using WorkOps.Models.Enums;

namespace WorkOps.Components.Pages.TReportPages;

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
    /// 日付
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 日付文字列
    /// </summary>
    [Display(Name = "日付")]
    public string DateString { get; set; } = string.Empty;

    /// <summary>
    /// 作業内容
    /// </summary>
    [Display(Name = "作業内容")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 詳細入力モデルリスト
    /// </summary>
    public List<InputDetailModel> InputDetailModels { get; set; } = default!;

    /// <summary>
    /// 承認ステータス
    /// </summary>
    public ApprovalStatus ApprovalStatus { get; set; }

    /// <summary>
    /// クリックできるステータス
    /// </summary>
    public List<ApprovalStatus> ClickableStatuses { get; set; } = [];
}