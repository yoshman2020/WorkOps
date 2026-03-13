using Microsoft.EntityFrameworkCore;
using WorkOps.Data;

namespace WorkOps.Services;

/// <summary>
/// 稼働日
/// </summary>
/// <param name="db">ApplicationDbContext</param>
public class BusinessDayService(ApplicationDbContext db)
{
    /// <summary>
    /// 前稼働日取得
    /// </summary>
    /// <param name="date">当日</param>
    /// <returns>前稼働日</returns>
    public async Task<DateTime> GetPreviousBusinessDay(DateTime date)
    {
        // 祝日
        var holidays = await db.MHoliday
            .Select(x => x.Date)
            .ToListAsync();

        var holidaySet = holidays.ToHashSet();

        var d = date.Date.AddDays(-1);

        // 土日祝日を除く前日
        while (d.DayOfWeek == DayOfWeek.Saturday ||
               d.DayOfWeek == DayOfWeek.Sunday ||
               holidaySet.Contains(DateOnly.FromDateTime(d)))
        {
            d = d.AddDays(-1);
        }

        return d;
    }

    /// <summary>
    /// 稼働日判定
    /// </summary>
    /// <param name="date">判定日</param>
    /// <returns>true:稼働日/false:休日</returns>
    public async Task<bool> IsBusinessDay(DateTime date)
    {
        // 土日
        if (date.DayOfWeek == DayOfWeek.Saturday ||
            date.DayOfWeek == DayOfWeek.Sunday)
            return false;

        var day = DateOnly.FromDateTime(date);

        // 祝日
        return !await db.MHoliday.AnyAsync(x => x.Date == day);
    }

    /// <summary>
    /// 作業工数（期間内の勤務時間）取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="start">開始日時</param>
    /// <param name="end">終了日時</param>
    /// <returns>作業工数（期間内の勤務時間）</returns>
    public async Task<TimeSpan> CalculateWorkDurationAsync(
        string userId, DateTime start, DateTime end)
    {
        if (start >= end) return TimeSpan.Zero;

        // ユーザーの勤務時間を取得
        var user = await db.Users
            .Include(u => u.MWorkTime)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.MWorkTime == null) return TimeSpan.Zero;

        var config = user.MWorkTime;
        // デフォルト値の適用（null許容型や初期値未設定への対策）
        TimeOnly workStart = config.StartTime;      // 09:00
        TimeOnly workEnd = config.EndTime;          // 18:00
        TimeOnly breakStart = config.BreakStartTime;// 12:00
        TimeOnly breakEnd = config.BreakEndTime;    // 13:00

        // 期間内の祝日リストを取得
        var holidayDates = await db.MHoliday
            .Where(h => h.Date >= DateOnly.FromDateTime(start)
                && h.Date <= DateOnly.FromDateTime(end))
            .Select(h => h.Date)
            .ToHashSetAsync();

        TimeSpan totalDuration = TimeSpan.Zero;

        // 1日ずつループして計算
        for (var date = DateOnly.FromDateTime(start);
            date <= DateOnly.FromDateTime(end);
            date = date.AddDays(1))
        {
            // 土日または祝日はスキップ
            if (date.DayOfWeek == DayOfWeek.Saturday
                || date.DayOfWeek == DayOfWeek.Sunday
                || holidayDates.Contains(date))
                continue;

            // その日の勤務可能枠をDateTimeに変換
            DateTime dayWorkStart = date.ToDateTime(workStart);
            DateTime dayWorkEnd = date.ToDateTime(workEnd);
            DateTime dayBreakStart = date.ToDateTime(breakStart);
            DateTime dayBreakEnd = date.ToDateTime(breakEnd);

            // 実際の勤務開始/終了（入力されたstart/endと、定時枠の重なる部分）
            DateTime actualStart = start > dayWorkStart ? start : dayWorkStart;
            DateTime actualEnd = end < dayWorkEnd ? end : dayWorkEnd;

            if (actualStart < actualEnd)
            {
                // 基礎勤務時間
                TimeSpan dayDuration = actualEnd - actualStart;

                // 休憩時間の重なりを計算して差し引く
                DateTime overlapBreakStart = actualStart > dayBreakStart
                    ? actualStart : dayBreakStart;
                DateTime overlapBreakEnd = actualEnd < dayBreakEnd
                    ? actualEnd : dayBreakEnd;

                if (overlapBreakStart < overlapBreakEnd)
                {
                    dayDuration -= (overlapBreakEnd - overlapBreakStart);
                }

                totalDuration += dayDuration;
            }
        }

        return totalDuration;
    }
}
