using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace WorkOps.Extensions;

/// <summary>
/// Enum拡張メソッド
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// 表示名を取得する
    /// </summary>
    /// <param name="value">Enum</param>
    /// <param name="isShortName">短縮名を使用するかどうか</param>
    /// <returns>表示名</returns>
    public static string GetDisplayName(this Enum value, bool isShortName = false)
    {
        var member = value.GetType().GetMember(value.ToString()).First();
        var attr = member.GetCustomAttribute<DisplayAttribute>();
        if (isShortName)
        {
            return attr?.ShortName ?? value.ToString();
        }
        return attr?.Name ?? value.ToString();
    }

    /// <summary>
    /// 値をEnumに変換する
    /// </summary>
    /// <typeparam name="TEnum">Enumの型</typeparam>
    /// <param name="value">文字列の値</param>
    /// <returns>Enumの値</returns>
    public static TEnum ToEnum<TEnum>(this object? value) where TEnum : struct, Enum
    {
        if (value == null)
        {
            return default;
        }
        try
        {
            if (value is int intValue)
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
            }

            if (value is string stringValue
                && Enum.TryParse<TEnum>(stringValue, true, out var result))
            {
                return result;
            }
            return default;
        }
        catch
        {
            return default;
        }
    }
}
