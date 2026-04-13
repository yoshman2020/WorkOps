using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using WorkOps.Data;

namespace WorkOps.Services
{
    /// <summary>
    /// ユーザー関連のサービス
    /// </summary>
    /// <param name="dbContext">DBコンテキスト</param>
    public class UserService(ApplicationDbContext dbContext,
        AuthenticationStateProvider authenticationStateProvider)
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly AuthenticationStateProvider _authenticationStateProvider
            = authenticationStateProvider;
        /// <summary>
        /// Adminロールを持たないユーザーの一覧を取得します。
        /// </summary>
        /// <param name="isDisplayDeleted">削除済みユーザーも表示</param>
        /// <returns>ユーザー一覧</returns>
        public async Task<List<ApplicationUser>> GetUsersAsync(
            bool isDisplayDeleted = false)
        {
            var users = _dbContext.Users
                .Where(u =>
                    !_dbContext.UserRoles.Any(ur => ur.UserId == u.Id &&
                        _dbContext.Roles.Any(
                                r => r.Id == ur.RoleId && r.Name == "Admin"))
                    );
            if (!isDisplayDeleted)
            {
                users = users.Where(u => u.IsDeleted == false);
            }

            return await users
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        /// <summary>
        /// ログインユーザーのID取得
        /// </summary>
        /// <returns>ログインユーザーのID</returns>
        public async Task<string> GetUserIdAsync()
        {
            var authState = await _authenticationStateProvider
                .GetAuthenticationStateAsync();
            var user = authState.User;
            return user.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
        }

        /// <summary>
        /// ユーザー名取得
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>ユーザー名</returns>
        public string GetUserName(string userId)
        {
            return _dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => u.FullName)
                .FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// ユーザーID取得
        /// </summary>
        /// <param name="users">ユーザー一覧</param>
        /// <param name="id">ユーザーID</param>
        /// <returns>ユーザーID</returns>
        public async Task<string> GetUserIdAsync(
            List<ApplicationUser> users, string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                // IDがすでに設定されている場合はその値
                return id;
            }

            var authState = await _authenticationStateProvider
                .GetAuthenticationStateAsync();
            var user = authState.User;

            string userId;

            if (user?.Identity?.IsAuthenticated ?? false)
            {
                // 認証されている場合そのID
                var authUserId = user.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authUserId))
                {
                    authUserId = users.FirstOrDefault()?.Id ?? string.Empty;
                }
                userId = authUserId;
            }
            else
            {
                // 認証されていない場合は最初のユーザー
                userId = users.FirstOrDefault()?.Id ?? string.Empty;
            }
            return userId;
        }

        /// <summary>
        /// ログイン中のユーザーが管理者権限を持つかどうかを確認します。
        /// </summary>
        /// <returns>管理者権限を持っている場合true</returns>
        public async Task<bool> HasAdminRoleAsync()
        {
            var authState = await _authenticationStateProvider
                .GetAuthenticationStateAsync();
            var user = authState.User;

            return user.IsInRole("Admin") || user.IsInRole("Manager");
        }

        /// <summary>
        /// 役職
        /// </summary>
        public static readonly List<KeyValuePair<string, string>> Roles
            = [
            new("User", "一般ユーザー"),
            new("Manager", "管理者")
        ];

        /// <summary>
        /// 役職名
        /// </summary>
        /// <param name="role">役職</param>
        /// <returns>役職名</returns>
        public static string GetRoleName(string role)
        {
            return Roles.FirstOrDefault(r => r.Key == role).Value ?? string.Empty;
        }

        /// <summary>
        /// 役職名
        /// </summary>
        /// <param name="roles">役職</param>
        /// <returns>役職名</returns>
        public static string GetRoleName(IList<string> roles)
        {
            foreach (var role in Roles)
            {
                if (roles.Contains(role.Key))
                {
                    return role.Value;
                }
            }
            return string.Empty;
        }
    }
}
