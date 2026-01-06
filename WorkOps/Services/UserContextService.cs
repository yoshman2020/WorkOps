using Microsoft.AspNetCore.Components.Authorization;

namespace WorkOps.Services;

/// <summary>
/// ログインユーザーのコンテキスト情報を提供するサービスの実装
/// </summary>
/// <param name="httpContextAccessor"></param>
public class UserContextService(
    IHttpContextAccessor httpContextAccessor,
    AuthenticationStateProvider authStateProvider,
    ILogger<UserContextService> logger)
{

    public async Task<string?> GetUserIdAsync()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            logger.LogWarning("HttpContext.User is null");
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            user = authState.User;
            if (user == null)
            {
                logger.LogWarning("AuthenticationState.User is null");
                return null;
            }
        }
        return user.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst("oid")?.Value
                ?? user?.FindFirst("sub")?.Value;
    }

    public async Task<string?> GetUserNameAsync()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            logger.LogWarning("HttpContext.User is null");
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            user = authState.User;
            if (user == null)
            {
                logger.LogWarning("AuthenticationState.User is null");
                return null;
            }
        }
        return user.Identity?.Name;
    }
}
