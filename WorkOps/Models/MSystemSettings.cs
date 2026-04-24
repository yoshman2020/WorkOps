namespace WorkOps.Models;

/// <summary>
/// システム設定
/// </summary>
public class MSystemSettings : BaseEntity
{
    /// <summary>
    /// 承認ステータスが「提出済」になったときにメールを送信するかどうか
    /// </summary>
    public bool IsSendSubmittedStatusMail { get; set; } = false;
}
