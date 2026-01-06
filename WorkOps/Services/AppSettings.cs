namespace WorkOps.Services
{
    public class AppSettings
    {
        public string SiteTitle { get; set; } = string.Empty;

        public string SiteDescription { get; set; } = string.Empty;

        public int ItemsPerPage { get; set; } = 10;

        /// <summary>
        /// 会社名
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// 出退勤とログイン・ログアウトの時間差（分）
        /// </summary>
        public int LoginBufferMin { get; set; } = 15;
    }
}
