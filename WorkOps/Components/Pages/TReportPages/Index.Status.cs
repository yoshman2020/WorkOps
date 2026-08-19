using ClosedXML.Excel;
using WorkOps.Extensions;
using WorkOps.Models;
using WorkOps.Models.Enums;

namespace WorkOps.Components.Pages.TReportPages;

/// <summary>
/// 承認ステータス処理部分
/// </summary>
public partial class Index
{
    /// <summary>
    /// 承認ステータスを更新する
    /// </summary>
    /// <param name="status">更新するステータス</param>
    /// <returns></returns>
    private async Task UpdateStatusAsync(ApprovalStatus status, int reportId)
    {
        var treport = await DbContext.TReport.FindAsync(reportId);
        if (treport is null)
        {
            return;
        }
        treport.MApprovalStatusId = status == ApprovalStatus.NotSubmitted
            ? null : (int)status;
        await DbContext.SaveChangesAsync();
        await LoadSelectedDataAsync();
    }

    private async Task<List<ApprovalStatus>> GetClickableStatusesAsync(
        TReport treport)
    {
        List<ApprovalStatus> statuses = [];
        if (treport.Id == 0)
        {
            // 作成されていない場合はクリック不可
            return statuses;
        }

        var status = EnumExtensions.ToEnum<ApprovalStatus>(treport.MApprovalStatusId);

        if (await UserService.HasAdminRoleAsync())
        {
            // 管理者権限ありの場合は、提出済⇔承認済
            switch (status)
            {
                case ApprovalStatus.SubmittedPendingManager:
                    statuses.Add(ApprovalStatus.Approved);
                    break;
                case ApprovalStatus.Approved:
                    statuses.Add(ApprovalStatus.SubmittedPendingManager);
                    break;
            }
        }
        var loginUserId = await UserService.GetUserIdAsync();
        if (UserId == loginUserId)
        {
            // 自身のデータの場合は、未提出⇔提出済
            switch (status)
            {
                case ApprovalStatus.NotSubmitted:
                    statuses.Add(ApprovalStatus.SubmittedPendingManager);
                    break;
                case ApprovalStatus.SubmittedPendingManager:
                    statuses.Add(ApprovalStatus.NotSubmitted);
                    break;
            }
        }

        return statuses;
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
        var userLastName = !string.IsNullOrEmpty(userName)
            ? userName.Split([' ', '　'])[0] : string.Empty;

        // Word生成
        using var wordMs = await CreateWordMemoryStreamAsync(userLastName);
        if (wordMs == null)
        {
            // Word生成に失敗した場合はメール送信しない
            return;
        }

        // 進捗のExcel生成
        using var workbook = new XLWorkbook();

        using var excelMs = TPlanActualPages.Index.CreateExcelMemoryStream(
            UserService, DbContext, UserId,
            Month.Year, Month.Month, string.Empty,
            workbook);
        if (excelMs == null)
        {
            // Excel生成に失敗した場合はメール送信しない
            return;
        }

        // メール送信
        var users = await UserService.GetUsersAsync();
        // 週間報告書提出時にメール送信するユーザー、または自分自身に送信する
        var sendUsers = users.Where(u => u.IsSendReportEmail == true
                || u.Id == InputModels?.FirstOrDefault()?.UserId)
            .Select(u => u.Email);
        if (sendUsers is null || !sendUsers.Any())
        {
            // メール送信対象がいない場合はメール送信しない
            return;
        }
        await MailService.SendWithAttachmentAsync(sendUsers!,
            $"週間報告書（{userLastName} {Month:yyyy/MM/dd}）", "週間報告書送付",
            [
                (wordMs.ToArray(),
                    $"週間報告書{Month:yyyy-MM}({userLastName}).docx"),
                (excelMs.ToArray(),
                    $"{userLastName}スケジュール.xlsx")
            ]);
    }
}
