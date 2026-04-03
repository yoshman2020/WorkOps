namespace WorkOps.Models;

/// <summary>
/// 祝祭日
/// </summary>
public class MHoliday : BaseEntity
{
    /// <summary>
    /// 日付
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 祝祭日
    /// </summary>
    public MHoliday() : base()
    {
    }

    /// <summary>
    /// 祝祭日
    /// </summary>
    /// <param name="model"></param>
    public MHoliday(BaseInputModel model) : base(model)
    {
    }
}
