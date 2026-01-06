namespace WorkOps.Models;

/// <summary>
/// 顧客
/// </summary>
public class MCustomer : BaseEntity
{
    /// <summary>
    /// プロジェクトリスト
    /// </summary>
    public ICollection<MProject> MProjects { get; set; } = [];

    /// <summary>
    /// 顧客
    /// </summary>
    public MCustomer() : base()
    {
    }

    /// <summary>
    /// 顧客
    /// </summary>
    /// <param name="model"></param>
    public MCustomer(BaseInputModel model) : base(model)
    {
    }
}
