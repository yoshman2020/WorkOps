using Microsoft.EntityFrameworkCore;
using WorkOps.Extensions;
using WorkOps.Models;
using WorkOps.Models.Enums;
using WorkOps.Services;

namespace WorkOps.Components.Pages.TAttendancePages;

/// <summary>
/// DB処理
/// </summary>
public partial class Index
{
    /// <summary>
    /// データ読み込み
    /// </summary>
    /// <param name="from">開始日</param>
    /// <param name="to">終了日</param>
    /// <returns></returns>
    private async Task<List<InputModel>> LoadDataAsync(DateOnly from, DateOnly to)
    {
        // 出退勤
        var attendances = LoadAttendances(from, to);

        // 実績
        var actuals = LoadActuals(from, to);

        // 祝日
        holidays = LoadHolidays(from, to);

        // 実績Dictionary
        var actualLookup = BuildActualLookup(actuals, from, to);

        // 編集不可かどうか
        var (canEdits, isReadOnlyAll) = await GetCanEditAsync(from, to);

        var models = BuildInputModels(
            from, to, attendances, actualLookup, holidays,
            canEdits, isReadOnlyAll);
        // 合計
        CalculateTotals(models);

        // 有給残数
        await CalculatePaidLeaveRemainingAsync();

        return models;
    }

    /// <summary>
    /// 出退勤取得
    /// </summary>
    /// <param name="from">開始日</param>
    /// <param name="to">終了日</param>
    /// <returns></returns>
    private List<TAttendance> LoadAttendances(DateOnly from, DateOnly to)
    {
        return [.. DbContext.TAttendance
            .Include(e => e.User)
            .Include(e => e.User!.MWorkTime)
            .Where(e =>
                e.UserId == UserId &&
                from <= e.Date &&
                e.Date <= to)];
    }

    /// <summary>
    /// 実績取得
    /// </summary>
    /// <param name="from">開始日</param>
    /// <param name="to">終了日</param>
    /// <returns></returns>
    private List<TActual> LoadActuals(DateOnly from, DateOnly to)
    {
        return [.. DbContext.TActual
            .Include(e => e.MPhase)
            .ThenInclude(p => p.MProject)
            .ThenInclude(p => p.MCustomer)
            .Where(e =>
                e.UserId == UserId &&
                from <= DateOnly.FromDateTime(e.EndDate) &&
                DateOnly.FromDateTime(e.StartDate) <= to)];
    }

    /// <summary>
    /// 祝日取得
    /// </summary>
    /// <param name="from">開始日</param>
    /// <param name="to">終了日</param>
    /// <returns></returns>
    private Dictionary<DateOnly, MHoliday> LoadHolidays(DateOnly from, DateOnly to)
    {
        return DbContext.MHoliday
            .Where(h => from <= h.Date && h.Date <= to)
            .ToDictionary(h => h.Date);
    }

    /// <summary>
    /// 実績Dictionary生成
    /// </summary>
    /// <param name="actuals">実績</param>
    /// <param name="from">開始日</param>
    /// <param name="to">終了日</param>
    /// <returns></returns>
    private static Dictionary<DateOnly, List<TActual>> BuildActualLookup(
        List<TActual> actuals,
        DateOnly from,
        DateOnly to)
    {
        var dict = new Dictionary<DateOnly, List<TActual>>();

        foreach (var actual in actuals)
        {
            var start = DateOnly.FromDateTime(actual.StartDate);
            var end = DateOnly.FromDateTime(actual.EndDate);

            if (start < from) start = from;
            if (end > to) end = to;

            for (var day = start; day <= end; day = day.AddDays(1))
            {
                if (!dict.TryGetValue(day, out var list))
                {
                    list = [];
                    dict[day] = list;
                }

                list.Add(actual);
            }
        }

        return dict;
    }

    /// <summary>
    /// 編集不可Dictionary生成
    /// </summary>
    /// <param name="from">開始日</param>
    /// <param name="to">終了日</param>
    /// <returns>日付（年×100＋月）ごとの編集可能フラグと全て編集不可かどうか</returns>
    private async Task<(Dictionary<int, bool> canEdits,
        bool isReadOnlyAll)>
        GetCanEditAsync(DateOnly from, DateOnly to)
    {
        // 管理者かどうか
        var isAdmin = await UserService.HasAdminRoleAsync();
        // 自身のデータかどうか
        var loginUserId = await UserService.GetUserIdAsync();
        var isOwnData = UserId == loginUserId;

        // 管理者でなく、自身のデータでない場合は全て編集不可
        if (!isAdmin && !isOwnData)
        {
            return (new Dictionary<int, bool>(), true);
        }

        var fromYm = from.Year * 100 + from.Month;
        var toYm = to.Year * 100 + to.Month;

        var statuses = DbContext.TAttendanceStatus
            .Where(s => s.UserId == UserId
                && (s.Year * 100 + s.Month) <= toYm
                && (s.Year * 100 + s.Month) >= fromYm)
            .AsEnumerable()
            .Select(s =>
            {
                var status = EnumExtensions.ToEnum<ApprovalStatus>(
                    s.MApprovalStatusId);
                var canEdit = ApprovalStatusService.CanEdit(
                    status, isAdmin, isOwnData);
                return (s.Year * 100 + s.Month, canEdit);
            })
            .ToDictionary(x => x.Item1, x => x.Item2)
            ;

        return (statuses, false);
    }

    /// <summary>
    /// InputModel作成
    /// </summary>
    /// <param name="from">開始日</param>
    /// <param name="to">終了日</param>
    /// <param name="attendances">出退勤</param>
    /// <param name="actuals">実績</param>
    /// <param name="holidays">祝日</param>
    /// <param name="canEdits">編集可能かどうか</param>
    /// <param name="isReadOnlyAll">全て編集不可かどうか</param>
    /// <returns></returns>
    private List<InputModel> BuildInputModels(
        DateOnly from,
        DateOnly to,
        List<TAttendance> attendances,
        Dictionary<DateOnly, List<TActual>> actualLookup,
        Dictionary<DateOnly, MHoliday> holidays,
        Dictionary<int, bool> canEdits,
        bool isReadOnlyAll)
    {
        var attendanceDict = attendances
            .GroupBy(a => a.Date)
            .ToDictionary(g => g.Key, g => g.First());

        var models = new List<InputModel>();

        for (var day = from; day <= to; day = day.AddDays(1))
        {
            attendanceDict.TryGetValue(day, out var attendance);

            actualLookup.TryGetValue(day, out var dayActuals);

            dayActuals ??= [];

            var model = new InputModel
            {
                Id = attendance?.Id ?? 0,
                UserId = UserId,
                UserName = attendance?.User?.FullName
                    ?? Users.FirstOrDefault(u => u.Id == UserId)?.FullName
                    ?? "",
                Date = day,
                HolidayName = holidays.TryGetValue(day, out var h)
                    ? h.Name
                    : "",
                StartTime = attendance?.StartTime,
                EndTime = attendance?.EndTime,
                PaidLeaveDuration = attendance?.PaidLeaveDuration,
                WorkedDuration = attendance?.WorkedDuration,
                OvertimeDuration = attendance?.OvertimeDuration,
                PaidLeaveDurationString =
                    GetDulationString(day, attendance?.PaidLeaveDuration),
                WorkedDurationString =
                    GetDulationString(day, attendance?.WorkedDuration),
                OvertimeDurationString =
                    GetDulationString(day, attendance?.OvertimeDuration),
                WorkDetailAm = string.Join(",",
                    dayActuals
                        .Where(a => a.StartDate.TimeOfDay <= new TimeSpan(12, 0, 0))
                        .Select(a =>
                            $"{a.MPhase.MProject.Name} {a.MPhase.Name}")),
                WorkDetailPm = string.Join(",",
                    dayActuals
                        .Where(a => new TimeSpan(12, 0, 0) < a.EndDate.TimeOfDay)
                        .Select(a =>
                            $"{a.MPhase.MProject.Name} {a.MPhase.Name}")),
                LoginTime = attendance?.LoginTime,
                LogoutTime = attendance?.LogoutTime,
                CanEdit = !isReadOnlyAll
                    && (!canEdits.TryGetValue(day.Year * 100 + day.Month,
                        out var canEdit) || canEdit),
            };

            models.Add(model);
        }

        return models;
    }

    /// <summary>
    /// 合計計算
    /// </summary>
    /// <param name="models"></param>
    private void CalculateTotals(List<InputModel> models)
    {
        totalPaidLeave = GettotalTime(models, e => e.PaidLeaveDuration);
        totalWorked = GettotalTime(models, e => e.WorkedDuration);
        totalOvertime = GettotalTime(models, e => e.OvertimeDuration);

        var days = models.Count(e => e.StartTime != null);

        workingDays = $"{days}";
        totalTime = $"{days * 8}時間";
    }

    /// <summary>
    /// 有給残数計算
    /// </summary>
    private async Task CalculatePaidLeaveRemainingAsync()
    {
        var user = Users.FirstOrDefault(u => u.Id == UserId);
        if (user == null)
        {
            return;
        }

        // DBから有給休暇日数を取得
        var grantedDays = DbContext.TPaidLeave
            .Where(pl => pl.UserId == UserId && pl.GrantedDate <= DateTo)
            .Sum(pl => pl.GrantedDays);

        // DBから有給休暇使用日数を取得
        var durations = await DbContext.TAttendance
            .Where(a => a.UserId == UserId && a.Date <= DateTo)
            .Select(a => a.PaidLeaveDuration)
            .ToListAsync();

        var usedDays = durations
            .Sum(d => d != null ? (decimal)d.Value.TotalHours / 8 : 0);

        var remainingDays = grantedDays - usedDays;

        paidLeaveRemaining = $"{remainingDays}日";
    }

    /// <summary>
    /// TimeSpanをDateTimeに変換
    /// </summary>
    /// <param name="date"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    private static string GetDulationString(DateOnly date, TimeSpan? timeSpan)
    {
        if (timeSpan == null || timeSpan == TimeSpan.Zero)
        {
            return string.Empty;
        }
        return date
            .ToDateTime(TimeOnly.MinValue)
            .Add(timeSpan ?? TimeSpan.Zero)
            .ToString("HH:mm");
    }

    /// <summary>
    /// TimeSpanの合計をDateTimeで返す
    /// </summary>
    /// <param name="entities">合計する列を含むエンティティ</param>
    /// <param name="selector">合計する列のセレクタ</param>
    /// <returns></returns>
    private static string GettotalTime<T>(
        IEnumerable<T> entities, Func<T, TimeSpan?> selector)
    {
        var total = entities
            .Select(selector)
            .Aggregate(TimeSpan.Zero, (sum, d) => sum + (d ?? TimeSpan.Zero));

        return $"{(int)total.TotalHours:00}:{total.Minutes:00}";
    }

    #region Notifications
    /// <summary>
    /// 前日・先週の未入力通知
    /// </summary>
    /// <returns></returns>
    private async Task<string> LoadNotificationsAsync()
    {
        // 前日・先週の未入力を通知する
        var isPrevReminderEnabled = DbContext.Users
            .Where(u => u.Id == UserId).FirstOrDefault()?
            .IsPrevReminderEnabled ?? false;

        if (!isPrevReminderEnabled)
        {
            // 通知しない場合
            return "";
        }

        var messages = new List<string>();

        var isExistPreviousBusinessDayActual = await PrevInputCheckService
            .ExistsPreviousBusinessDayActualAsync(UserId);

        if (!isExistPreviousBusinessDayActual)
        {
            messages.Add("前日の実績がありません。");
        }

        var isExistLastWeekReport = await PrevInputCheckService
            .ExistsLastWeekReportAsync(UserId);

        if (!isExistLastWeekReport)
        {
            messages.Add("先週の報告がありません。");
        }

        return string.Join(Environment.NewLine, messages);
    }
    #endregion // Notifications

    #region Status
    /// <summary>
    /// 1ヶ月表示かどうか
    /// </summary>
    /// <returns></returns>
    private bool IsDisplayOneMonth()
    {
        return DateTo == DateFrom.AddMonths(1).AddDays(-1);
    }

    /// <summary>
    /// 最終営業日を取得
    /// </summary>
    /// <param name="year">対象の年</param>
    /// <param name="month">対象の月</param>
    /// <param name="holidays">祝日リスト</param>
    /// <returns>最終営業日</returns>
    /// <exception cref="Exception"></exception>
    private static DateOnly GetLastBusinessDay(
        int year, int month, Dictionary<DateOnly, MHoliday> holidays)
    {
        var lastDay = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

        for (var date = lastDay; date.Day > 0; date = date.AddDays(-1))
        {
            if (IsBusinessDay(date, holidays))
            {
                return date;
            }
        }

        throw new Exception("営業日が存在しません");
    }

    /// <summary>
    /// 営業日かどうかを判定
    /// </summary>
    /// <param name="date">判定対象の日付</param>
    /// <param name="holidays">祝日リスト</param>
    /// <returns>営業日であればtrue、それ以外はfalse</returns>
    private static bool IsBusinessDay(
        DateOnly date, Dictionary<DateOnly, MHoliday> holidays)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday ||
            date.DayOfWeek == DayOfWeek.Sunday)
            return false;

        if (holidays.ContainsKey(date))
            return false;

        return true;
    }

    /// <summary>
    /// 承認ステータス取得
    /// </summary>
    /// <returns>承認ステータス</returns>
    private ApprovalStatus GetCurrentStatus()
    {
        if (!IsDisplayOneMonth())
        {
            // 1ヶ月表示でない場合
            return default;
        }

        var statusId = DbContext.TAttendanceStatus
            .Where(s => s.UserId == UserId
                && s.Year == DateFrom.Year
                && s.Month == DateFrom.Month)
            .Select(s => s.MApprovalStatusId)
            .FirstOrDefault();
        return EnumExtensions.ToEnum<ApprovalStatus>(statusId);
    }

    /// <summary>
    /// クリックできるステータス取得
    /// </summary>
    /// <returns>クリックできるステータス</returns>
    private async Task<List<ApprovalStatus>> GetClickableStatusesAsync()
    {
        List<ApprovalStatus> statuses = [];

        // 最終営業日
        var lastBusinessDay = GetLastBusinessDay(
            DateTo.Year, DateTo.Month, holidays);

        if (DateOnly.FromDateTime(DateTime.Today) < lastBusinessDay)
        {
            // 最終営業日前は押下不可
            return statuses;
        }

        if (await UserService.HasAdminRoleAsync())
        {
            // 管理者権限ありの場合は、提出済⇔管理者確認済、担当者確認済⇔承認済
            switch (currentStatus)
            {
                case ApprovalStatus.NotSubmitted:
                    break;
                case ApprovalStatus.SubmittedPendingManager:
                    statuses.Add(ApprovalStatus.UnderReviewByStaff);
                    break;
                case ApprovalStatus.UnderReviewByStaff:
                    statuses.Add(ApprovalStatus.SubmittedPendingManager);
                    break;
                case ApprovalStatus.ReviewedPendingApproval:
                    statuses.Add(ApprovalStatus.Approved);
                    break;
                case ApprovalStatus.Approved:
                    statuses.Add(ApprovalStatus.ReviewedPendingApproval);
                    break;
            }
        }

        var loginUserId = await UserService.GetUserIdAsync();
        if (UserId == loginUserId)
        {
            // 自身のデータの場合は、未提出⇔提出済、管理者確認済⇔担当者確認済
            switch (currentStatus)
            {
                case ApprovalStatus.NotSubmitted:
                    statuses.Add(ApprovalStatus.SubmittedPendingManager);
                    break;
                case ApprovalStatus.SubmittedPendingManager:
                    statuses.Add(ApprovalStatus.NotSubmitted);
                    break;
                case ApprovalStatus.UnderReviewByStaff:
                    statuses.Add(ApprovalStatus.ReviewedPendingApproval);
                    break;
                case ApprovalStatus.ReviewedPendingApproval:
                    statuses.Add(ApprovalStatus.UnderReviewByStaff);
                    break;
                case ApprovalStatus.Approved:
                    break;
            }
        }
        return statuses;
    }

    /// <summary>
    /// ステータス更新
    /// </summary>
    /// <param name="status"></param>
    /// <returns>更新後のステータス</returns>
    private async Task<ApprovalStatus> UpdateStatusAsync(ApprovalStatus status)
    {
        if (DbContext.TAttendanceStatus
            .Where(s => s.UserId == UserId
                && s.Year == DateFrom.Year
                && s.Month == DateFrom.Month)
            .FirstOrDefault() is TAttendanceStatus attendanceStatus)
        {
            // 既存のステータスがある場合
            if (status == ApprovalStatus.NotSubmitted)
            {
                // 未提出に戻す場合はレコードを削除
                DbContext.TAttendanceStatus.Remove(attendanceStatus);
            }
            else
            {
                // それ以外はステータスを更新
                attendanceStatus.MApprovalStatusId = (int)status;
            }
        }
        else
        {
            if (status == ApprovalStatus.NotSubmitted)
            {
                // 未提出のステータスはレコードを作成しない
                return status;
            }

            // 既存のステータスがない場合は新規作成
            DbContext.TAttendanceStatus.Add(new TAttendanceStatus
            {
                UserId = UserId,
                Year = DateFrom.Year,
                Month = DateFrom.Month,
                MApprovalStatusId = (int)status,
            });
        }
        await DbContext.SaveChangesAsync();

        return status;
    }
    #endregion // Status
}
