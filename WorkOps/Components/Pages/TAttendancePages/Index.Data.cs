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
            .Include(e => e.TAttendanceDetail)
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

            // 作業内容
            var workDetailAm = attendance?.TAttendanceDetail is null
                ? string.Join(",",
                    dayActuals
                        .Where(a => a.StartDate.TimeOfDay <= new TimeSpan(12, 0, 0))
                        // その他の場合は工程のみを表示
                        .Select(a => a.MPhase.MProject.MCustomer.Name == "その他"
                            ? a.MPhase.Name
                            : $"{a.MPhase.MProject.Name} {a.MPhase.Name}"))
                : attendance.TAttendanceDetail.WorkDetailAm
                ;
            var workDetailPm = attendance?.TAttendanceDetail is null
                ? string.Join(",",
                    dayActuals
                        .Where(a => new TimeSpan(12, 0, 0) < a.EndDate.TimeOfDay)
                        // その他の場合は工程のみを表示
                        .Select(a => a.MPhase.MProject.MCustomer.Name == "その他"
                            ? a.MPhase.Name
                            : $"{a.MPhase.MProject.Name} {a.MPhase.Name}"))
                : attendance.TAttendanceDetail.WorkDetailPm;

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
                WorkDetailAm = workDetailAm,
                WorkDetailPm = workDetailPm,
                LoginTime = attendance?.LoginTime,
                LogoutTime = attendance?.LogoutTime,
                IsModified = attendance?.IsModified ?? false,
                IsApproved = attendance?.IsApproved ?? false,
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
        else
        {
            var isPreviousBusinessDayActualMatching = await PrevInputCheckService
                .IsPreviousBusinessDayActualMatchingAsync(UserId);
            if (!isPreviousBusinessDayActualMatching)
            {
                messages.Add("前日の勤務時間と実績入力の時間が一致しません。");
            }
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
}
