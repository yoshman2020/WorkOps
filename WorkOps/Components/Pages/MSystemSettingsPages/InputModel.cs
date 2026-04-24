using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.MSystemSettingsPages;

/// <summary>
/// 入力モデル
/// </summary>
public sealed class InputModel : BaseInputModel
{
    /// <summary>
    /// 承認ステータスが「提出済」になったときにメールを送信するかどうか
    /// </summary>
    [Display(Name = "提出時にメールを送信する")]
    public bool IsSendSubmittedStatusMail { get; set; } = false;
}