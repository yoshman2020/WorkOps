using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WorkOps.Data;

namespace WorkOps.Services;

/// <summary>
/// 出退勤
/// </summary>

public class AttendanceRepository(ApplicationDbContext DbContext,
    IOptions<AppSettings> AppSettings)
{
    /// <summary>
    /// 出退勤レコードの登録・更新
    /// </summary>
    /// <param name="email">ユーザー名</param>
    /// <param name="isLogin">ログインの場合true</param>
    /// <returns></returns>
    public async Task UpsertAsync(string userId, bool isLogin = true)
    {
        if (userId == null)
        {
            return;
        }

        var now = DateTime.Now;
        var nowTimeOnly = TimeOnly.FromDateTime(now);
        var nowDateOnly = DateOnly.FromDateTime(now);

        // 勤務時間
        var workTime = DbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => u.MWorkTime)
            .FirstOrDefault();

        // 開始時間
        DateTime? startTime = null;
        // 終了時間
        DateTime? endTime = null;

        if (workTime != null)
        {
            if (isLogin)
            {
                // ログイン時が開始日時より前の場合、開始日時を設定
                if (nowTimeOnly < workTime.StartTime.AddMinutes(
                    AppSettings.Value.LoginBufferMin))
                {
                    startTime = nowDateOnly.ToDateTime(workTime.StartTime);
                }
            }
            else
            {
                // ログアウト時が終了時間より後の場合、終了時間を設定
                if (workTime.EndTime.AddMinutes(
                    -AppSettings.Value.LoginBufferMin) < nowTimeOnly)
                {
                    endTime = nowDateOnly.ToDateTime(workTime.EndTime);
                }
            }
        }

        // 出退勤レコードが存在しない場合、新規作成
        var exists = await DbContext.TAttendance
            .AnyAsync(a => a.UserId == userId
                    && a.Date == DateOnly.FromDateTime(now));

        if (!exists)
        {
            DateTime? loginTime = isLogin ? now : null;
            DateTime? logoutTime = isLogin ? null : now;

            var attendance = new Models.TAttendance
            {
                UserId = userId,
                Date = DateOnly.FromDateTime(now),
                LoginTime = loginTime,
                LogoutTime = logoutTime,
                StartTime = startTime,
                EndTime = endTime,
                Name = now.ToString("yyyy/MM/dd(ddd)"),
            };
            DbContext.TAttendance.Add(attendance);
            await DbContext.SaveChangesAsync();
        }
        else
        {
            var attendance = await DbContext.TAttendance
                .FirstOrDefaultAsync(a => a.UserId == userId
                    && a.Date == DateOnly.FromDateTime(now));

            if (attendance == null)
            {
                return;
            }

            // 出退勤レコードが存在する場合
            if (isLogin)
            {
                // すでにログインしている場合は何もしない
                var existsLogin = await DbContext.TAttendance
                    .AnyAsync(a => a.UserId == userId
                        && a.Date == DateOnly.FromDateTime(now)
                        && a.LoginTime != null);
                if (existsLogin)
                {
                    return;
                }

                // ログイン時間を更新
                attendance.LoginTime = now;
                attendance.StartTime ??= startTime;
            }
            else
            {
                // ログアウト時間を更新
                attendance.LogoutTime = now;
                attendance.EndTime ??= endTime;
            }

            if (workTime != null
                && attendance.StartTime != null
                && TimeOnly.FromDateTime(attendance.StartTime.Value)
                    == workTime.StartTime
                && attendance.EndTime != null
                && TimeOnly.FromDateTime(attendance.EndTime.Value)
                    == workTime?.EndTime)
            {
                // 出退勤が時間通りの場合勤務時間設定
                attendance.WorkedDuration ??= workTime?.WorkedDuration;
            }

            DbContext.TAttendance.Update(attendance);
            await DbContext.SaveChangesAsync();
        }
    }
}
