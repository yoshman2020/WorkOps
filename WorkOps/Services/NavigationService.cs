using DocumentFormat.OpenXml.Vml.Office;
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
        var currentUrl = Uri.EscapeDataString(_navigationManager.Uri);
        _navigationManager.NavigateTo(
            $"/{indexPage}/create?from={currentUrl}&{query}");
    }

    /// <summary>
    /// 元のページまたは一覧ページへ遷移
    /// </summary>
    /// <param name="from"></param>
    /// <param name="idColumn"></param>
    /// <param name="id"></param>
    /// <param name="indexPage"></param>
    public void NavigateToBack(string? from, string? idColumn, string? id,
        string indexPage)
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
            var queryDictionary = query.ToDictionary(
                x => x.Key, x => x.Value.ToString());

            // idが既にある場合は置き換える
            queryDictionary[idColumn] = id;

            var newUrl = QueryHelpers.AddQueryString(
                uri.GetLeftPart(UriPartial.Path), queryDictionary!);

            // 遷移してきた場合は、作成後に画面に戻る
            _navigationManager.NavigateTo(newUrl);
            return;
        }
        _navigationManager.NavigateTo($"/{indexPage}");
    }
}
