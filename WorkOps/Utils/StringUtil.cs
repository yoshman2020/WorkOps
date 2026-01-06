namespace WorkOps.Utils
{
    /// <summary>
    /// 文字列操作ユーティリティ
    /// </summary>
    public static class StringUtil
    {

        /// <summary>
        /// インデックス（0～）を全角数字（１～）に変換
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>１～の全角文字列</returns>
        public static string ConvertDigitsToFullWidth(int index)
        {
            string number = (index + 1).ToString();

            // 半角を全角に置換
            return number
                .Replace("0", "０")
                .Replace("1", "１")
                .Replace("2", "２")
                .Replace("3", "３")
                .Replace("4", "４")
                .Replace("5", "５")
                .Replace("6", "６")
                .Replace("7", "７")
                .Replace("8", "８")
                .Replace("9", "９");
        }
    }
}
