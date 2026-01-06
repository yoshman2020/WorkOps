using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkOps.Data;
using WorkOps.Utils;

namespace WorkOps.Models;

/// <summary>
/// 基本エンティティ
/// </summary>
public class BaseEntity
{
    /// <summary>
    /// ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required]
    [Display(Name = "名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 備考
    /// </summary>
    [Display(Name = "備考")]
    public string Remarks { get; set; } = string.Empty;

    /// <summary>
    /// 削除済み
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 登録日
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 登録者ID
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 更新日
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者ID
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// 登録者
    /// </summary>
    [ForeignKey("CreatedBy")]
    public ApplicationUser? CreatedUser { get; set; }

    /// <summary>
    /// 更新者
    /// </summary>
    [ForeignKey("UpdatedBy")]
    public ApplicationUser? UpdatedUser { get; set; }

    /// <summary>
    /// 基本エンティティ
    /// </summary>
    public BaseEntity() { }

    /// <summary>
    /// 基本エンティティ
    /// </summary>
    /// <param name="model">基本入力モデル</param>
    public BaseEntity(BaseInputModel model)
    {
        PropertyUtil.CopyProperties(model, this);
    }
}
