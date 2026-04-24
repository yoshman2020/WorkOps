using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.JSInterop;
using WorkOps.Data;
using WorkOps.Models;
using WorkOps.Models.Enums;
using WorkOps.Models.Errors;
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
#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                LoadSelectedDataAsync();
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
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
#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                LoadSelectedDataAsync();
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
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
#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                LoadSelectedDataAsync();
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
            }
        }
    }

    // 前日・先週の未入力通知
    private string notifications = string.Empty;

    // エラーコード
    private ErrorCode errorCode = ErrorCode.None;

    // 承認ステータス
    private ApprovalStatus currentStatus = ApprovalStatus.NotSubmitted;

    // クリックできるステータス
    private List<ApprovalStatus> clickableStatuses = [];

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
    // 有給残日数
    private string paidLeaveRemaining = string.Empty;

    /// <summary>
    /// 管理者権限なし
    /// </summary>
    private bool hassNotAuthorized = true;

    /// <summary>
    /// 休日リスト
    /// </summary>
    private Dictionary<DateOnly, MHoliday> holidays = [];

    /// <summary>
    /// 初期化
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        Logger.LogDebug("▽OnInitializedAsync");
        try
        {
            Users = await UserService.GetUsersAsync();
            UserId = await UserService.GetUserIdAsync(Users, UserId);

            // 管理者の場合承認可
            hassNotAuthorized = !await UserService.HasAdminRoleAsync();

            DateService.SetThisMonth(ref _dateFrom, ref _dateTo);
            await LoadSelectedDataAsync();

            // 前日・先週の未入力通知
            notifications = await LoadNotificationsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred!");
        }
        Logger.LogDebug("△OnInitializedAsync");
    }

    /// <summary>
    /// データ読み込み
    /// </summary>
    /// <returns></returns>
    private async Task LoadSelectedDataAsync()
    {
        Logger.LogDebug("▼LoadSelectedDataAsync");
        try
        {
            InputModels = await LoadDataAsync(DateFrom, DateTo);

            // 承認ステータス取得
            currentStatus = GetCurrentStatus();

            // クリックできるステータス取得
            clickableStatuses = await GetClickableStatusesAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred!");
        }
        Logger.LogDebug("▲LoadSelectedDataAsync");
    }

    /// <summary>
    /// 月を変更する
    /// </summary>
    /// <param name="offset">オフセット。0の場合は当月</param>
    private async Task ChangeMonthAsync(int offset)
    {
        DateService.ChangeMonth(offset, ref _dateFrom, ref _dateTo);
        await LoadSelectedDataAsync();
    }

    /// <summary>
    /// ステータスクリック
    /// </summary>
    /// <param name="status">更新するステータス</param>
    private async Task OnStepClickAsync(ApprovalStatus status)
    {
        Logger.LogDebug("▽OnStepClick");
        try
        {
            if (currentStatus == status || !clickableStatuses.Contains(status)
            || errorCode != ErrorCode.None)
            {
                Logger.LogDebug("△OnStepClick : Status is not allowed.");
                return;
            }

            // ステータス更新
            currentStatus = await UpdateStatusAsync(status);

            // メール送信
            await SendEmailAsync(status);

            // 再読込
            await LoadSelectedDataAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred!");
        }
        Logger.LogDebug("△OnStepClick");
    }

    /// <summary>
    /// Excel保存
    /// </summary>
    /// <returns></returns>
    private async Task DownloadExcelAsync()
    {
        Logger.LogDebug("▽DownloadExcelAsync");
        if (InputModels == null || InputModels.Count == 0)
        {
            return;
        }

        try
        {
            var userName = InputModels.FirstOrDefault()?.UserName;

            using var workbook = new XLWorkbook();

            using var excelMs = await CreateExcelMemoryStreamAsync(
                userName, workbook);
            using var streamRef = new DotNetStreamReference(stream: excelMs);

            var fileName = $"{DateFrom:yyyy}年勤務表_{userName}.xlsx";
            await JS.InvokeVoidAsync(
                "downloadFileFromStream", fileName, streamRef);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred!");
        }
        Logger.LogDebug("△DownloadExcelAsync");
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

    /// <summary>
    /// 承認チェックボックス変更時
    /// </summary>
    /// <param name="e">イベント引数</param>
    /// <param name="tattendance">対象行</param>
    /// <returns></returns>
    private async Task OnChkIsApprovedClickAsync(
        ChangeEventArgs e, InputModel tattendance)
    {
        Logger.LogDebug("▽OnChkIsApprovedClick");
        try
        {
            if (e.Value == null)
            {
                return;
            }
            tattendance.IsApproved = (bool)e.Value;
            errorCode = await InputModelService
                .SaveInputModelAsync<InputModel, TAttendance>(
                    tattendance, tattendance.Id, false);

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred!");
        }
        Logger.LogDebug("△OnChkIsApprovedClick");
    }
}
