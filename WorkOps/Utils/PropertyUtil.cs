namespace WorkOps.Utils
{
    /// <summary>
    /// プロパティ操作ユーティリティ
    /// </summary>
    public static class PropertyUtil
    {
        /// <summary>
        /// プロパティの値をコピーする
        /// </summary>
        /// <typeparam name="TSource">コピー元</typeparam>
        /// <typeparam name="TTarget">コピー先</typeparam>
        /// <param name="source">コピー元</param>
        /// <param name="target">コピー先</param>
        public static void CopyProperties<TSource, TTarget>(
            TSource source, TTarget target)
        {
            if (source is null || target is null)
            {
                return;
            }
            // source と target のプロパティ情報を取得
            var sourceProperties = source!.GetType().GetProperties();
            var targetProperties = target!.GetType().GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                // source のプロパティ名と一致する target のプロパティを探す
                var targetProperty = targetProperties
                    .FirstOrDefault(p => p.Name == sourceProperty.Name);

                if (targetProperty != null && targetProperty.CanWrite)
                {
                    // source と target の型が一致するか、互換性がある場合に値をコピー
                    if (targetProperty.PropertyType
                        .IsAssignableFrom(sourceProperty.PropertyType))
                    {
                        // 型が互換性があればコピー
                        var value = sourceProperty.GetValue(source);
                        targetProperty.SetValue(target, value);
                    }
                    else
                    {
                        // 型が異なる場合、型変換してコピーする
                        try
                        {
                            var value = sourceProperty.GetValue(source);
                            if (value != null)
                            {
                                var convertedValue = Convert.ChangeType(
                                    value, targetProperty.PropertyType);
                                targetProperty.SetValue(target, convertedValue);
                            }
                        }
                        catch (InvalidCastException)
                        {
                            // 型変換ができない場合はスキップ
                            Console.WriteLine(
                                $"Cannot convert {sourceProperty.Name} from {sourceProperty.PropertyType} to {targetProperty.PropertyType}");
                        }
                    }
                }
            }
        }
    }
}