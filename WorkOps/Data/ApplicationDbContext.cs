using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkOps.Models;
using WorkOps.Services;

namespace WorkOps.Data
{
    /// <summary>
    /// DBコンテキスト
    /// </summary>
    /// <param name="options">DBコンテキストのオプション</param>
    /// <param name="userContext">ログインユーザー取得コンテキスト</param>
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        UserContextService userContext,
        ILogger<ApplicationDbContext> logger
        )
        : IdentityDbContext<ApplicationUser>(options)
    {
        /// <summary>
        /// 勤務時間
        /// </summary>
        public DbSet<MWorkTime> MWorkTime { get; set; } = default!;

        /// <summary>
        /// 祝祭日
        /// </summary>
        public DbSet<MHoliday> MHoliday { get; set; } = default!;

        /// <summary>
        /// 顧客
        /// </summary>
        public DbSet<MCustomer> MCustomer { get; set; } = default!;

        /// <summary>
        /// プロジェクト
        /// </summary>
        public DbSet<MProject> MProject { get; set; } = default!;

        /// <summary>
        /// 工程
        /// </summary>
        public DbSet<MPhase> MPhase { get; set; } = default!;

        /// <summary>
        /// 予定
        /// </summary>
        public DbSet<TPlan> TPlan { get; set; } = default!;

        /// <summary>
        /// 実績
        /// </summary>
        public DbSet<TActual> TActual { get; set; } = default!;

        /// <summary>
        /// 出退勤
        /// </summary>
        public DbSet<TAttendance> TAttendance { get; set; } = default!;

        /// <summary>
        /// 週報
        /// </summary>
        public DbSet<TReport> TReport { get; set; } = default!;

        /// <summary>
        /// 週報詳細
        /// </summary>
        public DbSet<TReportDetail> TReportDetail { get; set; } = default!;

        /// <summary>
        /// 有給休暇
        /// </summary>
        public DbSet<TPaidLeave> TPaidLeave { get; set; } = default!;

        /// <summary>
        /// 承認ステータス
        /// </summary>
        public DbSet<MApprovalStatus> MApprovalStatus { get; set; } = default!;

        /// <summary>
        /// 出退勤ステータス
        /// </summary>
        public DbSet<TAttendanceStatus> TAttendanceStatus { get; set; } = default!;

        /// <summary>
        /// システム設定
        /// </summary>
        public DbSet<MSystemSettings> MSystemSettings { get; set; } = default!;

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>保存結果</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _ = ApplyAuditInformationAsync();
            logger.LogInformation("SaveChangesAsync");
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 保存時に監査情報を適用する
        /// </summary>
        private async Task ApplyAuditInformationAsync()
        {
            var now = DateTime.Now;
            string? userId = null;
            if (userContext != null)
            {
                userId = await userContext.GetUserIdAsync();
            }

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy ??= userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                }
            }
        }

        /// <summary>
        /// モデル作成時
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUserとMWorkTimeのリレーション設定が自動でできないため、
            // 明示的に設定する
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.MWorkTime)
                .WithMany(t => t.ApplicationUsers)
                .HasForeignKey(u => u.WorkTimeId);
        }
    }
}
