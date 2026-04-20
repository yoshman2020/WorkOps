namespace WorkOps.Models.Errors;

/// <summary>
/// エラーメッセージ
/// </summary>
public static class ErrorMessage
{
    /// <summary>
    /// エラーメッセージ取得
    /// </summary>
    /// <param name="code">エラーコード</param>
    /// <returns></returns>
    public static string GetMessage(ErrorCode code, params object[] args) =>
    string.Format(code switch
    {
        ErrorCode.None => "",
        ErrorCode.NotFound => "データが見つかりません。",
        ErrorCode.InvalidInput => "入力が不正です。",
        ErrorCode.Unauthorized => "権限がありません。",
        ErrorCode.Forbidden => "アクセスが禁止されています。",
        ErrorCode.Conflict => "データの競合が発生しました。",
        ErrorCode.InternalServerError => "サーバーエラーが発生しました。",
        ErrorCode.Duplicate => "データが重複しています。",
        ErrorCode.HasChildren => "関連する{0}が存在します。",
        ErrorCode.AlreadySubmitted => "提出済のため変更できません。",
        _ => "不明なエラーが発生しました。",
    }, args);
}
