using Microsoft.AspNetCore.Identity;

namespace WorkOps.Data;

public class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<
            RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<
            UserManager<ApplicationUser>>();
        var userStore = serviceProvider.GetRequiredService<
            IUserStore<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<
            ApplicationDbContext>();

        string[] roleNames = ["Admin", "Manager", "User"];

        foreach (var roleName in roleNames)
        {
            // ロールが存在しなければ作成
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 管理者ユーザー作成
        (var adminUser, string? userId) = await CreateUser(
            userManager, userStore,
            "admin@example.com", "Admin@123", "Admin", "システム管理者");

        // マネージャーユーザー作成
        (var managerUser, _) = await CreateUser(
            userManager, userStore,
            "manager@example.com", "Manager@123", "Manager", "管理者");

        // 一般ユーザー作成
        (var generalUser, _) = await CreateUser(
            userManager, userStore,
            "user@example.com", "User@123", "User", "一般ユーザー");

        if (dbContext.MWorkTime.Any())
        {
            return; // すでにデータが存在する場合はシード処理をスキップ
        }

        // 勤務時間
        dbContext.MWorkTime.AddRange(
            new Models.MWorkTime
            {
                Name = "標準",
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(18, 0),
                BreakStartTime = new TimeOnly(12, 0),
                BreakEndTime = new TimeOnly(13, 0),
                WorkedDuration = TimeSpan.FromHours(8),
                CreatedBy = userId,
            },
            new Models.MWorkTime
            {
                Name = "早出",
                StartTime = new TimeOnly(7, 20),
                EndTime = new TimeOnly(16, 5),
                BreakStartTime = new TimeOnly(12, 0),
                BreakEndTime = new TimeOnly(12, 45),
                WorkedDuration = TimeSpan.FromHours(8),
                CreatedBy = userId,
            }
        );

        // 祝祭日
        dbContext.MHoliday.AddRange(
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 1, 1),
                Name = "元旦",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 1, 13),
                Name = "成人の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 2, 11),
                Name = "建国記念日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 2, 23),
                Name = "天皇誕生日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 2, 24),
                Name = "振替休日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 3, 20),
                Name = "春分の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 4, 29),
                Name = "昭和の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 5, 3),
                Name = "憲法記念日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 5, 4),
                Name = "みどりの日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 5, 5),
                Name = "こどもの日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 5, 6),
                Name = "振替休日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 7, 21),
                Name = "海の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 8, 11),
                Name = "山の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 9, 15),
                Name = "敬老の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 9, 23),
                Name = "秋分の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 10, 13),
                Name = "スポーツの日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 11, 3),
                Name = "文化の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 11, 23),
                Name = "勤労感謝の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 11, 24),
                Name = "振替休日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 1, 2),
                Name = "年始休暇",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 1, 3),
                Name = "年始休暇",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 4, 28),
                Name = "会社休暇",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 12, 29),
                Name = "年末休暇",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 12, 30),
                Name = "年末休暇",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2025, 12, 31),
                Name = "年末休暇",
                CreatedBy = userId,
            },

            new Models.MHoliday
            {
                Date = new DateOnly(2026, 1, 1),
                Name = "元旦",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 1, 12),
                Name = "成人の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 2, 11),
                Name = "建国記念日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 2, 23),
                Name = "天皇誕生日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 3, 20),
                Name = "春分の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 4, 29),
                Name = "昭和の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 5, 3),
                Name = "憲法記念日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 5, 4),
                Name = "みどりの日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 5, 5),
                Name = "こどもの日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 5, 6),
                Name = "振替休日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 7, 20),
                Name = "海の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 8, 11),
                Name = "山の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 9, 21),
                Name = "敬老の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 9, 22),
                Name = "国民の休日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 9, 23),
                Name = "秋分の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 10, 12),
                Name = "スポーツの日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 11, 3),
                Name = "文化の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 11, 23),
                Name = "勤労感謝の日",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 1, 2),
                Name = "年始休暇",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 12, 29),
                Name = "年末休暇",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 12, 30),
                Name = "年末休暇",
                CreatedBy = userId,
            },
            new Models.MHoliday
            {
                Date = new DateOnly(2026, 12, 31),
                Name = "年末休暇",
                CreatedBy = userId,
            }
        );

        await dbContext.SaveChangesAsync();

        // ユーザーに勤務時間を割り当て
        var wirkTimeId = dbContext.MWorkTime.First().Id;
        adminUser.WorkTimeId = wirkTimeId;
        managerUser.WorkTimeId = wirkTimeId;
        generalUser.WorkTimeId = wirkTimeId;

        await dbContext.SaveChangesAsync();
    }

    private static async Task<(ApplicationUser user, string? userId)> CreateUser(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        string adminEmail, string adminPassword, string role, string fullName)
    {
        ApplicationUser? user;
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        var userId = adminUser?.Id;
        if (adminUser == null)
        {
            user = CreateUser();
            user.FullName = fullName;

            await userStore.SetUserNameAsync(user, adminEmail,
                CancellationToken.None);
            var emailStore = GetEmailStore(userManager, userStore);
            await emailStore.SetEmailAsync(user, adminEmail, CancellationToken.None);
            var result = await userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
            {
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmResult = await userManager.ConfirmEmailAsync(user, code);

                if (confirmResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    userId = user.Id;
                }
            }
        }
        else
        {
            user = adminUser;
        }

        return (user, userId);
    }

    private static ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
        }
    }

    private static IUserEmailStore<ApplicationUser> GetEmailStore(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore)
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)userStore;
    }
}
