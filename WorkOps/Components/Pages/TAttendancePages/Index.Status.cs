using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using WorkOps.Extensions;
using WorkOps.Models;
using WorkOps.Models.Enums;

namespace WorkOps.Components.Pages.TAttendancePages;

/// <summary>
/// 承認ステータス処理部分
/// </summary>
public partial class Index
{
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

        if (DateOnly.FromDateTime(DateTime.Today) < lastBusinessDay
            && !await DbContext.TAttendance
                .Where(t => t.UserId == UserId
                    && t.Date == lastBusinessDay)
                .AnyAsync())
        {
            // 最終営業日前で、最終営業日のデータが無いは押下不可
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

    /// <summary>
    /// メール送信
    /// </summary>
    /// <param name="status">ステータス</param>
    /// <returns></returns>
    /// <throws="InvalidOperationException">SMTPの設定が行われていない場合</exception>
    private async Task SendEmailAsync(ApprovalStatus status)
    {
        if (!(DbContext.MSystemSettings.FirstOrDefault()?
            .IsSendSubmittedStatusMail ?? false))
        {
            // メール送信設定が無効な場合はメール送信しない
            return;
        }

        if (status != ApprovalStatus.SubmittedPendingManager)
        {
            // 提出済以外はメール送信しない
            return;
        }

        var userName = InputModels?.FirstOrDefault()?.UserName ?? string.Empty;
        using var workbook = new XLWorkbook();

        // Excel生成
        using var excelMs = await CreateExcelMemoryStreamAsync(
            userName, workbook);
        if (excelMs == null)
        {
            // Excel生成に失敗した場合はメール送信しない
            return;
        }

        // メール送信
        await MailService.SendWithAttachmentAsync(
            "勤務表", "勤務表送付",
            [(excelMs.ToArray(),
            $"{DateFrom:yyyy}年勤務表_{userName}.xlsx")]);
    }
}
