using Microsoft.EntityFrameworkCore;
using WorkOps.Data;

namespace WorkOps.Services;

/// <summary>
/// 前日・先週の入力があるかチェック
/// </summary>
/// <param name="db">ApplicationDBContext</param>
/// <param name="businessDayService">稼働日</param>
public class PrevInputCheckService(
    ApplicationDbContext db,
    BusinessDayService businessDayService)
{
    /// <summary>
    /// 前営業日に実績入力があるか
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>前営業日に入力がある場合true</returns>
    public async Task<bool> ExistsPreviousBusinessDayActualAsync(string userId)
    {
        // 前営業日
        var prev = await businessDayService
            .GetPreviousBusinessDay(DateTime.Today);

        var start = prev.Date;
        var end = start.AddDays(1);

        // 前営業日の実績が存在するか
        return await db.TActual
            .AnyAsync(x => x.UserId == userId &&
                           x.StartDate < end &&
                           x.EndDate >= start);
    }

    /// <summary>
    /// 月曜日に、先週に週報入力があるか
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>月曜日（祝膣の場合は翌日）に、先週の週報入力がない場合false。
    /// 先週の入力があればtrue。月曜以外もtrue。</returns>
    public async Task<bool> ExistsLastWeekReportAsync(string userId)
    {
        var today = DateTime.Today;

        // 今週の月曜
        var monday = today.AddDays(-(int)today.DayOfWeek + 1);

        var firstBusinessDay = monday;

        while (!await businessDayService.IsBusinessDay(firstBusinessDay))
        {
            // 営業日ではない場合は次の日
            firstBusinessDay = firstBusinessDay.AddDays(1);
        }

        // 週の最初の営業日でなければチェック不要
        if (today.Date != firstBusinessDay.Date)
            return true;

        var start = DateOnly.FromDateTime(monday.AddDays(-7));
        var end = DateOnly.FromDateTime(monday);

        // 先週の週報が存在するか
        return await db.TReport
            .AnyAsync(x => x.UserId == userId &&
                           x.Date >= start &&
                           x.Date < end);
    }
}
