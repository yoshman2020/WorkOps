using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using WorkOps.Data;

namespace WorkOps.Services;

public class CustomCookieAuthenticationEvents(
    ILogger<CustomCookieAuthenticationEvents> logger,
    IServiceScopeFactory scopeFactory,
    IMemoryCache memoryCache,
    IOptions<AppSettings> appSettings
    ) : CookieAuthenticationEvents
{

    public override async Task ValidatePrincipal(
        CookieValidatePrincipalContext context)
    {
        try
        {
            var userId = context.Principal?.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var casheKey = $"AttendanceChecked_{userId}";

            if (memoryCache.TryGetValue(casheKey, out _))
            {
                return;
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<
                ApplicationDbContext>();

            // 出退勤レコード登録
            AttendanceRepository attendanceRepository
                = new(dbContext, appSettings);
            await attendanceRepository.UpsertAsync(userId, true);

            memoryCache.Set(casheKey, true, TimeSpan.FromHours(12));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in ValidatePrincipal");
            throw;
        }
        finally
        {
            await base.ValidatePrincipal(context);
        }
    }
}
