using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace WorkOps.Components;

/// <summary>
/// Display属性のNameを列タイトルに表示する列
/// </summary>
/// <typeparam name="TGridItem">グリッド行</typeparam>
/// <typeparam name="TProp">プロパティ</typeparam>
public class DisplayPropertyColumn<TGridItem, TProp> : PropertyColumn<TGridItem, TProp>
{
    private Func<TGridItem, TProp>? _compiledProperty;
    private bool _isBoolType;
    private bool _isDateTimeType;
    private bool _isDateOnlyType;
    private bool _isTimeOnlyType;

    /// <summary>
    /// Boolean true の表示文字列（デフォルト: "〇"）
    /// </summary>
    [Parameter]
    public string BoolTrueDisplay { get; set; } = "〇";

    /// <summary>
    /// Boolean false の表示文字列（デフォルト: "　"）
    /// </summary>
    [Parameter]
    public string BoolFalseDisplay { get; set; } = "　";

    /// <summary>
    /// 初期化
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Property == null) return;

        // プロパティを実行可能なデリゲートにコンパイル
        _compiledProperty = Property.Compile();

        var type = typeof(TProp);

        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Func<,>))
        {
            // Funcの戻り値の型を取得
            type = type.GetGenericArguments()[1];
        }

        // Nullable型の場合は基礎型を取得
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        _isBoolType = underlyingType == typeof(bool);
        _isDateTimeType = underlyingType == typeof(DateTime);
        _isDateOnlyType = underlyingType == typeof(DateOnly);
        _isTimeOnlyType = underlyingType == typeof(TimeOnly);

        // 各型のデフォルトフォーマットを設定
        if (string.IsNullOrEmpty(Format))
        {
            if (_isDateTimeType)
                Format = "yyyy/MM/dd HH:mm:ss";
            else if (_isDateOnlyType)
                Format = "yyyy/MM/dd";
            else if (_isTimeOnlyType)
                Format = "HH:mm:ss";
        }
    }

    /// <summary>
    /// パラメーターが設定されたときに呼び出されます。
    /// </summary>
    protected override void OnParametersSet()
    {
        if (Title is null && Property?.Body is MemberExpression memberExpression)
        {
            var propertyInfo = memberExpression.Expression?.Type.GetProperty(
                memberExpression.Member.Name);
            var label = propertyInfo?
                .GetCustomAttributes(typeof(DisplayAttribute), true)
                .Cast<DisplayAttribute>().FirstOrDefault()?.Name;
            label ??= propertyInfo?.Name;

            Title ??= label;
        }

        Sortable = true;

        base.OnParametersSet();
    }

    protected override void CellContent(RenderTreeBuilder builder, TGridItem item)
    {
        if (_compiledProperty == null)
        {
            base.CellContent(builder, item);
            return;
        }

        try
        {
            var value = _compiledProperty(item);
            var displayText = FormatValue(value);

            builder.OpenElement(0, "span");
            builder.AddContent(1, displayText);
            builder.CloseElement();
        }
        catch
        {
            builder.AddContent(0, "");
        }
    }

    private string FormatValue(object? value)
    {
        if (value == null)
            return string.Empty;

        // Bool型の場合
        if (_isBoolType)
        {
            return (bool)value ? BoolTrueDisplay : BoolFalseDisplay;
        }

        // DateTime型の場合
        if (_isDateTimeType && value is DateTime dateValue)
        {
            return dateValue.ToString(Format ?? "yyyy/MM/dd HH:mm:ss");
        }

        // DateOnly型の場合
        if (_isDateOnlyType && value is DateOnly dateOnlyValue)
        {
            return dateOnlyValue.ToString(Format ?? "yyyy/MM/dd");
        }

        // TimeOnly型の場合
        if (_isTimeOnlyType && value is TimeOnly timeOnlyValue)
        {
            return timeOnlyValue.ToString(Format ?? "HH:mm:ss");
        }

        return value.ToString() ?? string.Empty;
    }
}
