using ClosedXML.Excel;
using WorkOps.Models.Attributes;

namespace WorkOps.Components.Pages.TAttendancePages;

/// <summary>
/// EXCEL処理
/// </summary>
public partial class Index
{
    /// <summary>
    /// ExcelのMemoryStreamを生成
    /// </summary>
    /// <param name="userName">ユーザー名</param>
    /// <param name="workbook">Excelのワークブック</param>
    /// <returns>生成されたMemoryStream</returns>
    private async Task<MemoryStream> CreateExcelMemoryStreamAsync(
        string? userName, XLWorkbook workbook)
    {
        for (int month = 1; month <= 12; month++)
        {
            await GenerateExcelAsync(workbook, month, userName ?? string.Empty);
        }

        workbook.Worksheet($"{DateFrom.Month}月").SetTabActive();
        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// Excelの１シートを生成
    /// </summary>
    /// <param name="workbook">WorkBook</param>
    /// <param name="month">月（1～12）</param>
    /// <param name="userName">ユーザー名</param>
    private async Task GenerateExcelAsync(
        XLWorkbook workbook, int month, string userName)
    {
        var inputModels = await LoadDataAsync(
            new DateOnly(DateFrom.Year, month, 1),
            new DateOnly(DateFrom.Year, month,
                DateTime.DaysInMonth(DateFrom.Year, month)));

        // 必要な列をリストに入れる
        IEnumerable<ExcelModel>? excelModels =
            inputModels?.Select(im => new ExcelModel
            {
                Day = im.Date.Day,
                DayOfWeek = im.Date.ToString("ddd"),
                HolidayName = im.HolidayName ?? string.Empty,
                StartTime = im.StartTime,
                EndTime = im.EndTime,
                PaidLeaveDuration = im.PaidLeaveDuration,
                WorkedDuration = im.WorkedDuration,
                OvertimeDuration = im.OvertimeDuration,
                Remarks = im.Remarks,
                WorkDetailAm = im.WorkDetailAm,
                WorkDetailPm = im.WorkDetailPm,
            });

        var sheet = workbook.Worksheets.Add($"{month}月");
        sheet.SheetView.ZoomScale = 70;

        // フォント設定
        sheet.Style.Font.FontName = "ＭＳ Ｐゴシック";
        sheet.Style.Font.FontSize = 11;

        sheet.Cell("A1").Value = $"{DateFrom:yyyy}年{month}月";
        sheet.Cell("A1").Style.Font.Bold = true;

        sheet.Cell("C1").Value = "勤務時間表";
        sheet.Cell("C1").Style.Font.Bold = true;

        sheet.Cell("I1").Value = userName;
        sheet.Cell("I1").Style.Alignment.Horizontal
            = XLAlignmentHorizontalValues.Right;

        var startRow = 4;
        var startCol = 1;
        sheet.Cell(startRow, startCol).InsertData(excelModels!);

        var properties = typeof(ExcelModel).GetProperties();

        for (int i = 0; i < properties.Length; i++)
        {
            var prop = properties[i];

            if (prop.GetCustomAttributes(typeof(ExcelColumnAttribute), false)
                .FirstOrDefault() is ExcelColumnAttribute attr)
            {
                var targetCol = startCol + i;

                // ヘッダー（1行上）のテキストを書き換え
                sheet.Cell(startRow - 1, targetCol).Value = attr.Header;

                // 列幅の反映
                sheet.Column(targetCol).Width = attr.Width;
            }
        }

        // データの最終行と最終列を計算
        var fixedDays = 31;
        // 実際のデータ末尾
        var lastRowOfData = startRow + excelModels!.Count() - 1;
        // 常に31行分確保
        var lastRow = startRow + fixedDays - 1;
        var lastCol = startCol + typeof(ExcelModel).GetProperties().Length - 1;

        // 時刻フォーマット
        sheet.Range(startRow, startCol + 3, lastRow, startCol + 7)
            .Style.DateFormat.Format = "h:mm";

        // 合計
        sheet.Range(lastRow + 1, 1, lastRow + 1, 3).Merge();
        sheet.Cell(lastRow + 1, 1).Value = "合計";
        sheet.Cell(lastRow + 1, 6).Value = totalPaidLeave;
        sheet.Cell(lastRow + 1, 7).Value = totalWorked;
        sheet.Cell(lastRow + 1, 8).Value = totalOvertime;
        sheet.Cell(lastRow + 2, 3).Value = totalTime;
        sheet.Cell(lastRow + 2, 5).Value = "稼働日";
        sheet.Cell(lastRow + 2, 7).Value = workingDays;
        sheet.Cell(lastRow + 2, 8).Value = "有給残";
        sheet.Cell(lastRow + 2, 9).Value = paidLeaveRemaining;

        // --- スタイルの適用 ---

        // 行の高さ
        var rowHeight = 23.25;

        // ヘッダー行の中央揃え
        var headerRange = sheet.Range(startRow - 1, startCol, startRow - 1, lastCol);
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        sheet.Row(startRow - 1).Height = rowHeight;

        // テーブル全体の範囲を取得
        var tableRange = sheet.Range(startRow - 1, startCol, lastRow, lastCol);

        // 格子状に線を引く
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        for (int row = startRow; row <= lastRow; row++)
        {
            // 行の高さ
            sheet.Row(row).Height = rowHeight;

            if (lastRowOfData < row)
            {
                continue;
            }

            // 色
            // 曜日
            var dayOfWeek = sheet.Cell(row, 2).GetString();
            // 祝祭日
            var holyday = sheet.Cell(row, 3).GetString();
            if (dayOfWeek == "土" || dayOfWeek == "日"
                || !string.IsNullOrEmpty(holyday))
            {
                sheet.Cell(row, 2).Style.Fill
                    .SetBackgroundColor(XLColor.LightGray);
                sheet.Range(row, 4, row, 7).Style.Fill
                    .SetBackgroundColor(XLColor.LightGray);
                sheet.Range(row, 10, row, 11).Style.Fill
                    .SetBackgroundColor(XLColor.LightGray);
            }

            // 曜日の色
            var dayOfWeekCellColor = XLColor.Black;

            if (dayOfWeek == "土")
            {
                dayOfWeekCellColor = XLColor.Blue;
            }
            else if (dayOfWeek == "日")
            {
                dayOfWeekCellColor = XLColor.Red;
            }
            if (!string.IsNullOrEmpty(holyday))
            {
                // 祝祭日
                dayOfWeekCellColor = XLColor.Red;
                sheet.Range(row, 2, row, 3).Style.Fill
                    .SetBackgroundColor(XLColor.LightCyan);
            }

            sheet.Cell(row, 2).Style.Font.FontColor = dayOfWeekCellColor;
        }

        // 列
        sheet.Range(startRow, 2, lastRow, 3).Style.Alignment.Horizontal
            = XLAlignmentHorizontalValues.Center;
        sheet.Range(startRow, 4, lastRow, 8).Style.Alignment.Horizontal
            = XLAlignmentHorizontalValues.Right;

        // 合計行
        var totalRange = sheet.Range(lastRow + 1, 1, lastRow + 1, 8);
        totalRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        totalRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        totalRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        sheet.Row(lastRow + 1).Height = rowHeight;
        sheet.Cell(lastRow + 1, 1).Style.Alignment.Horizontal
            = XLAlignmentHorizontalValues.Center;
        sheet.Cell(lastRow + 1, 1).Style.Alignment.Vertical
            = XLAlignmentVerticalValues.Center;
        sheet.Range(lastRow + 1, 6, lastRow + 1, 8).Style.Alignment.Vertical
            = XLAlignmentVerticalValues.Center;
        sheet.Cell(lastRow + 2, 3).Style.Alignment.Horizontal
            = XLAlignmentHorizontalValues.Center;
        sheet.Cell(lastRow + 2, 7).Style.Alignment.Horizontal
            = XLAlignmentHorizontalValues.Right;

        // 印刷範囲
        var printRange = sheet.Range(1, 1, lastRow + 1, 9);
        sheet.PageSetup.PrintAreas.Add(printRange.RangeAddress.ToString());
        sheet.PageSetup.FitToPages(1, 1);

        // 余白の設定
        static double cmToInch(double cm) => cm / 2.54;
        sheet.PageSetup.Margins.Top = cmToInch(2);
        sheet.PageSetup.Margins.Bottom = cmToInch(1);
        sheet.PageSetup.Margins.Left = cmToInch(1.5);
        sheet.PageSetup.Margins.Right = cmToInch(1);
        sheet.PageSetup.Margins.Header = cmToInch(1.3);
        sheet.PageSetup.Margins.Footer = cmToInch(1.3);
    }
}
