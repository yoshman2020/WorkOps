using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WorkOps.Models;

/// <summary>
/// 基本入力モデル
/// </summary>
public class BaseInputModel
{
    /// <summary>
    /// ID
    /// </summary>
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
    /// 登録日
    /// </summary>
    [Display(Name = "登録日")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 登録者ID
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 登録者
    /// </summary>
    [Display(Name = "登録者")]
    public string? CreatedUserName { get; set; }

    /// <summary>
    /// 更新日
    /// </summary>
    [Display(Name = "更新日")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者ID
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// 更新者
    /// </summary>
    [Display(Name = "更新者")]
    public string? UpdatedUserName { get; set; }

    /// <summary>
    /// シャロ―コピー
    /// </summary>
    /// <returns>コピーしたモデル</returns>
    public BaseInputModel Clone()
    {
        return (BaseInputModel)this.MemberwiseClone();
    }

    /// <summary>
    /// 名前重複チェック
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dbContext">DBコンテキスト</param>
    /// <returns>名前が重複している場合true</returns>
    public async Task<bool> IsDuplicateAsync<TEntity>(DbContext dbContext)
        where TEntity : class
    {
        return await dbContext.Set<TEntity>()
            .AnyAsync(e =>
                EF.Property<string>(e, nameof(Name)) == this.Name &&
                EF.Property<int>(e, nameof(Id)) != this.Id
                );
    }
}
