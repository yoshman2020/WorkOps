using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.MProjectPages;

/// <summary>
/// 入力モデル
/// </summary>
public sealed class InputModel : BaseInputModel
{
    /// <summary>
    /// 顧客ID
    /// </summary>
    [Required]
    [Display(Name = "顧客")]
    public int? MCustomerId { get; set; }

    /// <summary>
    /// 顧客
    /// </summary>
    [Display(Name = "顧客")]
    public string? CustomerName { get; set; }
}