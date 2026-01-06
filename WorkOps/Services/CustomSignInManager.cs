using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using WorkOps.Data;

namespace WorkOps.Services;

public class CustomSignInManager(
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor contextAccessor,
    IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
    IOptions<IdentityOptions> optionsAccessor,
    ILogger<SignInManager<ApplicationUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<ApplicationUser> confirmation,
    IOptions<AppSettings> appSettings
    ) : SignInManager<ApplicationUser>(
        userManager,
        contextAccessor,
        claimsFactory,
        optionsAccessor,
        logger,
        schemes,
        confirmation
        )
{

    public override async Task<SignInResult> PasswordSignInAsync(
        string userName, string password,
        bool isPersistent, bool lockoutOnFailure)
    {
        var result = await base.PasswordSignInAsync(
            userName, password, isPersistent, lockoutOnFailure);

        if (result.Succeeded)
        {
            var user = await UserManager.FindByNameAsync(userName);
            if (user != null)
            {
                var dbContext = Context.RequestServices.GetRequiredService<
                    ApplicationDbContext>();

                // 出退勤レコードが存在しない場合、新規作成
                AttendanceRepository attendanceRepository
                    = new(dbContext, appSettings);
                await attendanceRepository.UpsertAsync(user.Id, true);
            }
        }

        return result;
    }

    public override async Task SignOutAsync()
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            var dbContext = Context.RequestServices.GetRequiredService<
                ApplicationDbContext>();

            // 出退勤レコードが存在しない場合、新規作成
            AttendanceRepository attendanceRepository
                = new(dbContext, appSettings);
            await attendanceRepository.UpsertAsync(userId, false);
        }

        await base.SignOutAsync();
    }
}
