namespace WorkOps.Models.Errors;

public enum ErrorCode
{
    /// <summary>
    /// エラーなし
    /// </summary>
    None = 0,

    /// <summary>
    /// データなし
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// 入力不正
    /// </summary>
    InvalidInput = 2,

    /// <summary>
    /// 権限なし
    /// </summary>
    Unauthorized = 3,

    /// <summary>
    /// ページなし
    /// </summary>
    Forbidden = 4,

    /// <summary>
    /// 競合
    /// </summary>
    Conflict = 5,

    /// <summary>
    /// サーバーエラー
    /// </summary>
    InternalServerError = 6,

    /// <summary>
    /// 重複
    /// </summary>
    Duplicate = 7,

    /// <summary>
    /// 子要素あり
    /// </summary>
    HasChildren = 8,
}
