namespace WorkOps.Services
{
    public class DateService
    {
        /// <summary>
        /// 今月を設定
        /// </summary>
        /// <param name="dateFrom">開始日</param>
        /// <param name="dateTo">終了日</param>
        public static void SetThisMonth(ref DateOnly dateFrom, ref DateOnly dateTo)
        {
            if (dateFrom != default && dateTo != default
                && dateFrom != DateOnly.MinValue && dateTo != DateOnly.MinValue)
            {
                // すでに設定されている場合はそのまま
                return;
            }
            var today = DateTime.Today;
            dateFrom = new DateOnly(today.Year, today.Month, 1);
            dateTo = dateFrom.AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// 月を変更する
        /// </summary>
        /// <param name="offset">オフセット。0の場合は当月</param>
        /// <param name="dateFrom">開始日</param>
        public static void ChangeMonth(int offset,
            ref DateOnly dateFrom)
        {
            if (offset == 0)
            {
                // 0の場合は当月
                dateFrom = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
            }
            else
            {
                // 0以外の場合は開始日の1日＋オフセット
                dateFrom = new DateOnly(dateFrom.Year, dateFrom.Month, 1).AddMonths(offset);
            }
        }

        /// <summary>
        /// 月を変更する
        /// </summary>
        /// <param name="offset">オフセット。0の場合は当月</param>
        /// <param name="dateFrom">開始日</param>
        /// <param name="dateTo">終了日</param>
        public static void ChangeMonth(int offset,
            ref DateOnly dateFrom, ref DateOnly dateTo)
        {
            ChangeMonth(offset, ref dateFrom);
            dateTo = dateFrom.AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// 与えられた日付がその週の月曜日でない場合、月曜日を取得する
        /// </summary>
        /// <param name="date">日付</param>
        /// <returns>月曜日</returns>
        public static DateOnly GetStartOfWeek(DateOnly date)
        {
            // date.DayOfWeek は 0 (Sunday) から 6 (Saturday) までの値を取る
            int daysToMonday = (int)date.DayOfWeek - (int)DayOfWeek.Monday;

            // 月曜日に戻る日数を計算
            if (daysToMonday < 0)
                daysToMonday += 7;  // 日曜日の場合は 7 を足して調整

            return date.AddDays(-daysToMonday); // 月曜日の日付を取得
        }
    }
}
