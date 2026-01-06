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
    ///// <summary>
    ///// クラス名関数
    ///// </summary>
    //[Parameter]
    //public Func<TGridItem, string>? ClassSelector { get; set; }

    /// <summary>
    /// 初期化
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        var type = Property.Type;
        if (type == null)
        {
            return;
        }

        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Func<,>))
        {
            // Funcの戻り値の型を取得
            type = type.GetGenericArguments()[1];
        }

        // Nullable型の場合は基礎型を取得
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(DateTime))
        {
            // 日付型の場合のデフォルトフォーマットを指定
            Format = "yyyy/MM/dd HH:mm:ss";
        }
    }

    /// <summary>
    /// パラメーターが設定されたときに呼び出されます。
    /// </summary>
    protected override void OnParametersSet()
    {
        if (Title is null && Property.Body is MemberExpression memberExpression)
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

    ///// <summary>
    ///// セルのレンダリング
    ///// </summary>
    ///// <param name="builder"></param>
    ///// <param name="item"></param>
    //protected override void CellContent(RenderTreeBuilder builder, TGridItem item)
    //{
    //    // クラス名
    //    var className = ClassSelector?.Invoke(item);
    //    if (className == null)
    //    {
    //        // 設定されていない場合はそのまま
    //        base.CellContent(builder, item);
    //        return;
    //    }

    //    builder.OpenElement(0, "div");
    //    builder.AddAttribute(1, "class", className);

    //    base.CellContent(builder, item);

    //    builder.CloseElement(); // </div>

    //}
}
