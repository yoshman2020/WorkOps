using System.ComponentModel.DataAnnotations.Schema;
using WorkOps.Data;

namespace WorkOps.Models;

/// <summary>
/// 有給休暇
/// </summary>
public class TPaidLeave : BaseEntity
{
    /// <summary>
    /// 担当者ID
    /// </summary>
    [ForeignKey(nameof(ApplicationUser))]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 担当者
    /// </summary>
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// 付与日数
    /// </summary>
    public int GrantedDays { get; set; }

    /// <summary>
    /// 付与日
    /// </summary>
    public DateOnly GrantedDate { get; set; }

    /// <summary>
    /// 有効期限
    /// </summary>
    public DateOnly ExpiredDate { get; set; } = DateOnly.MaxValue;
}
