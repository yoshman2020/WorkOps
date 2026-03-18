using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using WorkOps.Data;
using WorkOps.Services;

namespace WorkOps.Components.Pages.TAttendancePages;

public partial class Index
{
    private string _userId = string.Empty;
    [SupplyParameterFromQuery]
    private string UserId
    {
        get => _userId;
        set
        {
            if (_userId != value)
            {
                _userId = value;
                LoadSelectedData();
            }
        }
    }
    private DateOnly _dateFrom;
    private DateOnly DateFrom
    {
        get => _dateFrom;
        set
        {
            if (_dateFrom != value)
            {
                _dateFrom = value;
                LoadSelectedData();
            }
        }
    }
    private DateOnly _dateTo;
    private DateOnly DateTo
    {
        get => _dateTo;
        set
        {
            if (_dateTo != value)
            {
                _dateTo = value;
                LoadSelectedData();
            }
        }
    }

    // 前日・先週の未入力通知
    private string notifications = string.Empty;
    private List<InputModel>? InputModels;
    private readonly PaginationState pagination = new() { ItemsPerPage = 31 };
    private List<ApplicationUser> Users = [];

    // 合計有給時間
    private string totalPaidLeave = string.Empty;
    // 合計勤務時間
    private string totalWorked = string.Empty;
    // 合計時間外
    private string totalOvertime = string.Empty;
    // 合計時間
    private string totalTime = string.Empty;
    // 稼働日
    private string workingDays = "0";

    /// <summary>
    /// 管理者権限なし
    /// </summary>
    private bool hassNotAuthorized = true;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        Users = await UserService.GetUsersAsync();
        UserId = await UserService.GetUserIdAsync(Users, UserId);

        // 管理者の場合承認可
        hassNotAuthorized = !await UserService.HasAdminRoleAsync();

        DateService.SetThisMonth(ref _dateFrom, ref _dateTo);
        LoadSelectedData();

        // 前日・先週の未入力通知
        notifications = await LoadNotificationsAsync();
    }

    /// <summary>
    /// データ読み込み
    /// </summary>
    /// <returns></returns>
    private void LoadSelectedData()
    {
        InputModels = LoadData(DateFrom, DateTo);
    }

    /// <summary>
    /// 月を変更する
    /// </summary>
    /// <param name="offset">オフセット。0の場合は当月</param>
    private void ChangeMonth(int offset)
    {
        DateService.ChangeMonth(offset, ref _dateFrom, ref _dateTo);
        LoadSelectedData();
    }

    /// <summary>
    /// 行クラス取得
    /// 土日の場合のCSSクラス設定
    /// </summary>
    /// <param name="tattendance">行</param>
    /// <returns>行クラス</returns>
    private static string? GetRowClass(InputModel tattendance)
    {
        var dow = tattendance.Date.DayOfWeek;

        return dow switch
        {
            DayOfWeek.Saturday => "saturday",
            DayOfWeek.Sunday => "sunday",
            _ => string.IsNullOrEmpty(tattendance.HolidayName) ? null : "holiday"
        };
    }

    /// <summary>
    /// 更新または新規作成画面遷移URL
    /// </summary>
    /// <param name="tattendance">行</param>
    /// <returns>更新または新規作成画面遷移URL</returns>
    private string GetEditOrCreateUrl(InputModel tattendance)
    {
        if (tattendance.Id == 0)
        {
            return "tattendances/create?userid=" + UserId
            + "&date=" + tattendance.Date;
        }
        return "tattendances/edit?id=" + tattendance.Id;
    }

}
