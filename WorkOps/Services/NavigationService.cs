using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace WorkOps.Services;

/// <summary>
/// ナビゲーションサービス
/// </summary>
/// <param name="navigationManager"></param>
public class NavigationService(NavigationManager navigationManager)
{
    /// <summary>
    /// ナビゲーションマネージャー
    /// </summary>
    private readonly NavigationManager _navigationManager = navigationManager;

    /// <summary>
    /// 新規作成ページへ遷移
    /// </summary>
    /// <param name="indexPage">一覧ページ</param>
    /// <param name="query">クエリパラメータ</param>
    public void NavigateToCreatet(string indexPage, string query = "")
    {
        var currentUrl = _navigationManager.Uri;

        // from パラメータ用に URL エンコード
        var queryDict = new Dictionary<string, string?>
        {
            ["from"] = currentUrl
        };

        // 呼び出し元から追加クエリがある場合は解析して追加
        if (!string.IsNullOrWhiteSpace(query))
        {
            // query が "key=value&key2=value2" の形式で来る前提
            var extraParams = QueryHelpers.ParseQuery(query)
                .ToDictionary(
                    x => x.Key.ToLower(),
                    x => x.Value.ToString()
                );

            foreach (var kvp in extraParams)
            {
                queryDict[kvp.Key] = kvp.Value;
            }
        }

        // NavigateTo 用 URL を生成
        var newUrl = QueryHelpers.AddQueryString(
            $"/{indexPage}/create", queryDict);

        _navigationManager.NavigateTo(newUrl);
    }

    /// <summary>
    /// 元のページまたは一覧ページへ遷移
    /// </summary>
    /// <param name="from">遷移元のURL</param>
    /// <param name="idColumn">ID列</param>
    /// <param name="id">ID</param>
    /// <param name="indexPage">一覧ページ</param>
    /// <param name="clearOtherIds">クリアするID列</param>
    public void NavigateToBack(string? from, string? idColumn, string? id,
        string indexPage,
        string[]? clearOtherIds = null)
    {
        if (!string.IsNullOrEmpty(from))
        {
            // idが無い場合は、作成後に画面に戻る
            if (string.IsNullOrEmpty(idColumn) || string.IsNullOrEmpty(id))
            {
                _navigationManager.NavigateTo(from);
                return;
            }

            var uri = new Uri(from, UriKind.RelativeOrAbsolute);
            var query = QueryHelpers.ParseQuery(uri.Query);
            // 全て小文字キーに変換
            var queryDictionary = query
                .ToDictionary(
                    x => x.Key.ToLower(),
                    x => x.Value.ToString()
                );

            // idが既にある場合は置き換える
            queryDictionary[idColumn] = id;

            // 呼び出し元から指定されたIDを0にする
            if (clearOtherIds != null)
            {
                foreach (var key in clearOtherIds)
                {
                    if (!string.Equals(key, idColumn,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        queryDictionary[key.ToLower()] = "0";
                    }
                }
            }

            var newUrl = QueryHelpers.AddQueryString(
                uri.GetLeftPart(UriPartial.Path), queryDictionary!);

            // 遷移してきた場合は、作成後に画面に戻る
            _navigationManager.NavigateTo(newUrl);
            return;
        }
        _navigationManager.NavigateTo($"/{indexPage}");
    }
}
