using ClosedXML.Excel;
using WorkOps.Data;
using WorkOps.Services;

namespace WorkOps.Components.Pages.TPlanActualPages;

/// <summary>
/// EXCEL処理
/// </summary>
public partial class Index
{
    /// <summary>
    /// ExcelのMemoryStreamを生成
    /// </summary>
    /// <param name="userService">ユーザーサービス</param>
    /// <param name="dbContext">DBコンテキスト</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="year">年</param>
    /// <param name="selectedMonth">選択された月</param>
    /// <param name="projectFilter">プロジェクトフィルター</param>
    /// <param name="isOnlyFromTo">期間中の工程のみ表示するか</param>
    /// <param name="workbook">Excelのワークブック</param>
    /// <returns>生成されたMemoryStream</returns>
    public static MemoryStream CreateExcelMemoryStream(
        UserService userService, ApplicationDbContext dbContext, string userId,
        int year, int selectedMonth, string projectFilter, bool isOnlyFromTo,
        XLWorkbook workbook)
    {
        for (int month = 1; month <= 12; month++)
        {
            GenerateExcel(userService, dbContext, userId,
                workbook, year, month, projectFilter, isOnlyFromTo);
        }

        workbook.Worksheet($"{selectedMonth}月").SetTabActive();
        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// Excelの１シートを生成
    /// </summary>
    /// <param name="userServicer">ユーザーサービス</param>
    /// <param name="dbContext">DBコンテキスト</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="workbook">WorkBook</param>
    /// <param name="year">年</param>
    /// <param name="month">月（1～12）</param>
    /// <param name="projectFilter">プロジェクトフィルター</param>
    /// <param name="isOnlyFromTo">期間中の工程のみ表示するか</param>
    /// <retruns>結果 true:OK false:NG</retruns>
    public static void GenerateExcel(
        UserService userServicer, ApplicationDbContext dbContext, string userId,
        XLWorkbook workbook, int year, int month,
        string projectFilter, bool isOnlyFromTo)
    {
        var (inputModels, dates) = LoadData(
            userServicer, dbContext, userId,
            new DateOnly(year, month, 1),
            new DateOnly(year, month,
                DateTime.DaysInMonth(year, month)),
            projectFilter, isOnlyFromTo);

        if (inputModels == null
            || dates == null || !dates!.Any())
        {
            throw new Exception("データの取得に失敗しました");
        }

        // 不要行削除
        inputModels = [.. inputModels.Where(i => i.RowClass != "d-none")];

        var grayColor = XLColor.LightGray;
        // 列幅 = センチメートル * 3.31889943
        // 行高さ = センチメートル * 37.7007874015748

        var sheet = workbook.Worksheets.Add($"{month}月");
        sheet.SheetView.ZoomScale = 100;

        // フォント設定
        sheet.Style.Font.FontName = "ＭＳ Ｐゴシック";
        sheet.Style.Font.FontSize = 11;
        sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        sheet.Column(1).Width = 3;
        sheet.Column(2).Width = 40.38;
        sheet.Column(3).Width = 8.38;
        sheet.Column(4).Width = 8.38;
        sheet.Column(5).Width = 6;

        // 日付列
        const int StartColumn = 6;
        // データ行
        const int StartRow = 4;
        // 最終列
        var lastColumn = dates.Count() + StartColumn - 1;
        // 最終行
        var lastRow = 0 < inputModels.Count
            ? inputModels.Count + StartRow - 1 : StartRow;

        sheet.Cell(1, 1).Value = $"{month}月 業務スケジュール";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Cell(1, 1).Style.Fill.BackgroundColor = grayColor;

        sheet.Cell(1, 6).Value = $"{year}年{month}月";
        sheet.Cell(1, 6).Style.Alignment.Horizontal
            = XLAlignmentHorizontalValues.Left;
        sheet.Cell(1, 6).Style.Font.Bold = true;
        sheet.Range(1, 6, 1, lastColumn).Style.Fill.BackgroundColor = grayColor;
        sheet.Range(1, 6, 1, lastColumn).Style.Border.OutsideBorder
            = XLBorderStyleValues.Thin;

        // ヘッダー行
        sheet.Cell(StartRow - 1, 1).Value = "No.";
        sheet.Cell(StartRow - 1, 2).Style.Alignment.Horizontal
            = XLAlignmentHorizontalValues.Left;
        sheet.Cell(StartRow - 1, 2).Value = "業務名、作業内容";
        sheet.Cell(StartRow - 1, 3).Value = "作業工数";
        sheet.Cell(StartRow - 1, 4).Value = "進捗率";
        sheet.Cell(StartRow - 1, 5).Value = "終了";
        sheet.Range(StartRow - 2, 1, StartRow - 1, lastColumn)
            .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        sheet.Range(StartRow - 2, 1, StartRow - 1, lastColumn)
            .Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        sheet.Range(StartRow, StartColumn, lastRow, lastColumn)
            .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        sheet.Range(StartRow, StartColumn, lastRow, lastColumn)
            .Style.Font.FontSize = 22;

        for (int col = StartColumn; col < dates.Count() + StartColumn; col++)
        {
            var date = dates!.ElementAt(col - StartColumn);
            sheet.Column(col).Width = 2.63;
            sheet.Cell(2, col).Value = date.Day;
            sheet.Cell(3, col).Value = $"{date:ddd}";

            // 休日の場合は灰色にする
            var dow = date.DayOfWeek;
            var isHoliday = dow switch
            {
                DayOfWeek.Saturday => true,
                DayOfWeek.Sunday => true,
                _ => dbContext.MHoliday
                    .Any(h => h.Date == DateOnly.FromDateTime(date))
            };
            if (isHoliday)
            {
                sheet.Range(StartRow - 2, col,
                    StartRow + inputModels.Count - 1, col)
                    .Style.Fill.BackgroundColor = grayColor;
            }
        }

        sheet.Range(1, 1, 2, 5).Merge();
        sheet.Range(1, 1, 2, 5).Style.Border.OutsideBorder
            = XLBorderStyleValues.Thick;
        sheet.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        sheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        sheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

        // 作業工数
        sheet.Range(StartRow, 3, lastRow, 3).Style.NumberFormat.Format = "0.0";
        // 終了
        sheet.Range(StartRow, 5, lastRow, 5).Style.DateFormat.Format = "MM/dd";

        // プロジェクト番号
        var projectNo = 1;

        for (int row = StartRow; row < inputModels.Count + StartRow; row++)
        {
            var inputModel = inputModels[row - StartRow];
            // 行の高さ
            var rowHeight = 19.22;
            if (inputModel.RowClass == "project"
                || inputModel.RowClass == "project2")
            {
                if (!string.IsNullOrEmpty(inputModel.PhaseName))
                {
                    // プロジェクト行
                    sheet.Cell(row, 1).Value = projectNo;
                    projectNo++;
                }
                sheet.Range(row, 1, row, lastColumn)
                    .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                sheet.Range(row, 1, row, lastColumn)
                    .Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                // 工数太字
                sheet.Cell(row, 3).Style.Font.Bold = true;
            }
            else
            {
                // 予定・実績行
                rowHeight /= 2;

                sheet.Range(row, 1, row, lastColumn)
                    .Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                sheet.Range(row, 1, row, lastColumn)
                    .Style.Border.RightBorder = XLBorderStyleValues.Thin;

                var color = XLColor.Blue;
                if (inputModel.IsActual)
                {
                    // 実績
                    color = XLColor.Red;
                    sheet.Range(row, 1, row, lastColumn)
                        .Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                }
                else
                {
                    // 予定
                    for (int col = 1; col < StartColumn; col++)
                    {
                        sheet.Range(row, col, row + 1, col).Merge();
                    }
                    sheet.Range(row, 1, row, lastColumn)
                        .Style.Border.TopBorder = XLBorderStyleValues.Thin;
                }
                sheet.Range(row, StartColumn, row, lastColumn)
                    .Style.Font.FontColor = color;
            }
            sheet.Row(row).Height = rowHeight;

            sheet.Cell(row, 2).Value = inputModel.PhaseName;
            if (inputModel.IsActual
                || (inputModel.RowClass == "project2"
                    && inputModel.PhaseName != "その他"))
            {
                // 実績行の場合、工数行がマージされているため行-1
                var manHourRow = inputModel.IsActual ? row - 1 : row;
                sheet.Cell(manHourRow, 3).Value = inputModel.PhaseTotalManHour;
                sheet.Cell(manHourRow, 4).Value = inputModel.ProgressRateString;
                sheet.Cell(manHourRow, 5).Value = inputModel.EndDate;
            }

            // 日付ごとの予定・実績
            for (int d = 0; d < dates.Count(); d++)
            {
                var date = dates.ElementAt(d);
                var (id, displayText, _) = inputModel.Cells[date];
                sheet.Cell(row, d + StartColumn).Value = displayText;
            }
        }
    }
}
