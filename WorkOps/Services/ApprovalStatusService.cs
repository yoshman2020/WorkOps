using WorkOps.Extensions;
using WorkOps.Models.Enums;

namespace WorkOps.Services;

/// <summary>
/// 承認ステータス関連サービス
/// </summary>
public class ApprovalStatusService(UserService userService)
{
    /// <summary>
    /// 編集可能かどうかを判定する
    /// </summary>
    /// <param name="status">承認ステータス</param>
    /// <param name="isAdmin">管理者かどうか</param>
    /// <param name="isOwnData">自身のデータかどうか</param>
    /// <returns>編集可能かどうか</returns>
    public static bool CanEdit(ApprovalStatus? status,
        bool isAdmin, bool isOwnData)
    {
        // 管理者でなく、自身のデータでない場合は編集不可
        if (!isAdmin && !isOwnData)
        {
            return false;
        }

        var canEdit = status switch
        {
            // ステータスがnullの場合は編集可能
            null => true,
            // 未提出は編集可能
            ApprovalStatus.NotSubmitted => true,
            // 提出済（管理者確認中）は管理者のみ編集可能
            ApprovalStatus.SubmittedPendingManager => isAdmin,
            // 管理者確認済（担当者確認中）は担当者のみ編集可能
            ApprovalStatus.UnderReviewByStaff => isOwnData,
            // 担当者確認済（承認待ち）は管理者のみ編集可能
            ApprovalStatus.ReviewedPendingApproval => isAdmin,
            // 承認済みは編集不可
            ApprovalStatus.Approved => false,
            _ => true,
        };
        return canEdit;
    }

    /// <summary>
    /// 編集可能かどうかを判定する（ステータスID版）
    /// </summary>
    /// <param name="statusId">承認ステータスID</param>
    /// <param name="isAdmin">管理者かどうか</param>
    /// <param name="isOwnData">自身のデータかどうか</param>
    /// <returns>編集可能かどうか</returns>
    public static bool CanEdit(int? statusId,
        bool isAdmin, bool isOwnData)
    {
        var status = EnumExtensions.ToEnum<ApprovalStatus>(statusId);
        return CanEdit(status, isAdmin, isOwnData);
    }

    /// <summary>
    /// 編集可能かどうかを非同期に判定する
    /// </summary>
    /// <param name="status">承認ステータス</param>
    /// <param name="userId">データのユーザーID</param>
    /// <returns>編集可能かどうか</returns>
    public async Task<bool> CanEditAsync(ApprovalStatus? status, string userId)
    {
        // 管理者かどうか
        var isAdmin = await userService.HasAdminRoleAsync();
        // 自身のデータかどうか
        var loginUserId = await userService.GetUserIdAsync();
        var isOwnData = userId == loginUserId;

        return CanEdit(status, isAdmin, isOwnData);
    }

    /// <summary>
    /// 編集可能かどうかを非同期に判定する（ステータスID版）
    /// </summary>
    /// <param name="statusId">承認ステータスID</param>
    /// <param name="userId">データのユーザーID</param>
    /// <returns>編集可能かどうか</returns>
    public async Task<bool> CanEditAsync(int? statusId, string userId)
    {
        var status = EnumExtensions.ToEnum<ApprovalStatus>(statusId);
        return await CanEditAsync(status, userId);
    }
}
