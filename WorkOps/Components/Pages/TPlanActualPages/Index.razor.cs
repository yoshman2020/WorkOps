using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using WorkOps.Data;
using WorkOps.Services;

namespace WorkOps.Components.Pages.TPlanActualPages;

public partial class Index
{
    private string _userId = string.Empty;
    [SupplyParameterFromQuery]
    private string UserId
    {
        get => _userId;
        set
        {
            if (_userId != value)
            {
                _userId = value;
                LoadSelectedData();
            }
        }
    }
    private DateOnly _dateFrom;
    private DateOnly DateFrom
    {
        get => _dateFrom;
        set
        {
            if (_dateFrom != value)
            {
                _dateFrom = value;
                LoadSelectedData();
            }
        }
    }
    private DateOnly _dateTo;
    private DateOnly DateTo
    {
        get => _dateTo;
        set
        {
            if (_dateTo != value)
            {
                _dateTo = value;
                LoadSelectedData();
            }
        }
    }

    private List<InputModel>? InputModels;
    private readonly PaginationState pagination = new() { ItemsPerPage = 30 };
    private List<ApplicationUser> Users = [];

    /// <summary>
    /// 期間内の日付一覧
    /// </summary>
    private IEnumerable<DateTime>? Dates = [];

    /// <summary>
    /// 予定、実績
    /// </summary>
    private static readonly bool[] planActArray = [false, true];

    /// <summary>
    /// 作業工数合計
    /// </summary>
    private double TotalManHour =>
        InputModels?
            .Where(x => x.IsActual)
            .Sum(x => x.ManHour) ?? 0;

    protected override async Task OnInitializedAsync()
    {
        Users = await UserService.GetUsersAsync();
        UserId = await UserService.GetUserIdAsync(Users, UserId);

        DateService.SetThisMonth(ref _dateFrom, ref _dateTo);
        LoadSelectedData();
    }

    /// <summary>
    /// データ読み込み
    /// </summary>
    /// <returns></returns>
    private void LoadSelectedData()
    {
        (InputModels, Dates) = LoadData(DateFrom, DateTo);
    }

    /// <summary>
    /// データ読み込み
    /// </summary>
    /// <param name="dtFrom">開始日</param>
    /// <param name="dtTo">終了日</param>
    /// <returns></returns>
    private (List<InputModel>?, IEnumerable<DateTime>?) LoadData(
        DateOnly? dtFrom, DateOnly? dtTo)
    {
        List<InputModel> inputModels = [];
        IEnumerable<DateTime> dates = [];
        try
        {
            if (dtFrom == default || dtTo == default
                || dtFrom == DateOnly.MinValue || dtTo == DateOnly.MinValue)
            {
                return (inputModels, dates);
            }

            var dateFrom = dtFrom!.Value;
            var dateTo = dtTo!.Value;

            var userName = UserService.GetUserName(UserId);

            dates = Enumerable
                .Range(0, dateTo.DayNumber - dateFrom.DayNumber + 1)
                .Select(offset => dateFrom.AddDays(offset).ToDateTime(
                    TimeOnly.MinValue))
                ;

            var fromDateTime = dateFrom.ToDateTime(TimeOnly.MinValue);
            var toDateTime = dateTo.ToDateTime(TimeOnly.MaxValue);

            // 期間内の工程
            var phases = DbContext.MPhase
                .Where(phase => DbContext.TPlan.Any(plan =>
                    (string.IsNullOrEmpty(UserId) || plan.UserId == UserId) &&
                    plan.MPhaseId == phase.Id &&
                    plan.StartDate <= toDateTime &&
                    plan.EndDate >= fromDateTime)
                    ||
                    DbContext.TActual.Any(actual =>
                    (string.IsNullOrEmpty(UserId) || actual.UserId == UserId) &&
                    actual.MPhaseId == phase.Id &&
                    actual.StartDate <= toDateTime &&
                    actual.EndDate >= fromDateTime))
                .Include(phase => phase.MProject)
                .ThenInclude(phase => phase.MCustomer)
                .OrderBy(e => e.MProject.MCustomerId)
                .ThenBy(e => e.MProjectId)
                .ThenBy(e => e.Id)
                ;

            // 予定と実績
            inputModels = [.. phases.AsEnumerable()
                .SelectMany(p => planActArray.Select(type => new InputModel
                    {
                        MPhaseId = p.Id,
                        MCustomerId = p.MProject!.MCustomerId,
                        MProjectId = p.MProjectId,
                        CustomerName = type ? "" : p.MProject.MCustomer!.Name,
                        ProjectName = type ? "" : p.MProject!.Name,
                        PhaseName = type ? "" : p.Name,
                        IsActual = type,
                        Cells = dates.ToDictionary(d =>
                            d, d => (0, string.Empty, string.Empty)),
                    })
                    .OrderBy(m => m.MCustomerId)
                    .ThenBy(m => m.MProjectId)
                    .ThenBy(m => m.MPhaseId)
                    .ThenBy(m => m.IsActual)
                )];

            // データなし
            if (inputModels.Count == 0)
            {
                return (inputModels, dates);
            }

            // 予定
            var plans = DbContext.TPlan
                .Where(e => string.IsNullOrEmpty(UserId) || e.UserId == UserId)
                .Where(e => (e.StartDate <= toDateTime && e.EndDate >= fromDateTime))
                .ToList();

            // 工程ごとにまとめた予定
            var groupedPlans = GroupContinuous(
                plans,
                x => x.StartDate,
                x => x.EndDate,
                x => x.MPhaseId);

            RenderCells(
                groupedPlans,
                plans,
                x => x.MPhaseId,
                x => x.StartDate,
                x => x.EndDate,
                x => x.Id,
                x => $"{x.MPhase!.MProject!.MCustomer!.Name} {x.MPhase.MProject.Name} {x.MPhase.Name}",
                false,
                inputModels,
                dates
            );

            // 実績
            var actuals = DbContext.TActual
                .Where(e => string.IsNullOrEmpty(UserId) || e.UserId == UserId)
                .Where(e => (e.StartDate <= toDateTime && e.EndDate >= fromDateTime))
                .ToList();

            // 工程ごとにまとめた実績
            var groupedActuals = GroupContinuous(
                actuals,
                x => x.StartDate,
                x => x.EndDate,
                x => x.MPhaseId);

            RenderCells(
                groupedActuals,
                actuals,
                x => x.MPhaseId,
                x => x.StartDate,
                x => x.EndDate,
                x => x.Id,
                x => $"{x.MPhase!.MProject!.MCustomer!.Name} {x.MPhase.MProject.Name} {x.MPhase.Name}",
                true,
                inputModels,
                dates,
                (actual, target) =>
                {
                    if (actual.ProgressRate != null)
                        target.ProgressRateString = $"{actual.ProgressRate} %";

                    if (actual.ProgressRate == 100)
                        target.EndDate = actual.EndDate;
                });

            // 工程別累計作業工数
            var phaseTotalManHours = DbContext.TActual
                .Where(e => string.IsNullOrEmpty(UserId) || e.UserId == UserId)
                .GroupBy(
                    a => a.MPhaseId,
                    a => a.ManHour
                )
                .ToDictionary(g => g.Key, g => g.Sum());

            // 作業工数
            foreach (var model in inputModels)
            {
                double totalManHour = 0;
                if (model.IsActual)
                {
                    totalManHour = actuals.AsEnumerable()
                        .Where(a => a.MPhaseId == model.MPhaseId)
                        .Sum(a => a.ManHour);
                }
                model.ManHour = totalManHour;

                if (model.IsActual)
                {
                    // 工程別累計作業工数
                    model.PhaseTotalManHour = phaseTotalManHours?
                        .GetValueOrDefault(model.MPhaseId ?? 0) ?? 0;
                }
            }

            // プロジェクトごとに２行ずつ追加
            var enhancedInputModels = new List<InputModel>();
            int? currentProjectId = inputModels[0].MProjectId;
            InputModel separatorModel = (inputModels[0].Clone() as InputModel)!;
            separatorModel.Cells = dates.ToDictionary(d => d,
                d => (0, string.Empty, string.Empty));
            separatorModel.PhaseName = "";
            // 1行目は非表示
            separatorModel.RowClass = "d-none";
            enhancedInputModels.Add(separatorModel);
            InputModel separatorModel2 = (separatorModel.Clone() as InputModel)!;
            separatorModel2.PhaseName =
                $"{separatorModel.CustomerName} {separatorModel.ProjectName}";
            separatorModel2.RowClass = "project2";
            enhancedInputModels.Add(separatorModel2);
            foreach (var model in inputModels)
            {
                if (model.MProjectId != currentProjectId)
                {
                    currentProjectId = model.MProjectId;
                    separatorModel = (model.Clone() as InputModel)!;
                    separatorModel.Cells = dates.ToDictionary(d => d,
                            d => (0, string.Empty, string.Empty));
                    separatorModel.PhaseName = "";
                    separatorModel.RowClass = "project";
                    enhancedInputModels.Add(separatorModel);
                    separatorModel2 = (separatorModel.Clone() as InputModel)!;
                    separatorModel2.PhaseName =
                        $"{separatorModel.CustomerName} {separatorModel.ProjectName}";
                    separatorModel2.RowClass = "project2";
                    enhancedInputModels.Add(separatorModel2);
                }
                model.RowClass = model.IsActual ? "actual" : "plan";
                model.PhaseName = string.IsNullOrEmpty(model.PhaseName)
                    ? string.Empty : $"・{model.PhaseName}";
                enhancedInputModels.Add(model);
            }

            return (enhancedInputModels, dates);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return (inputModels, dates);
        }
    }

    /// <summary>
    /// 連続した日付をグループ化する
    /// </summary>
    /// <typeparam name="T">データ型</typeparam>
    /// <param name="source">元データ</param>
    /// <param name="startSelector">開始日取得関数</param>
    /// <param name="endSelector">終了日取得関数</param>
    /// <param name="phaseSelector">工程ID取得関数</param>
    /// <returns>グループ化されたデータ</returns>
    private static List<(DateTime start, DateTime end, List<T> items)>
    GroupContinuous<T>(
        List<T> source,
        Func<T, DateTime> startSelector,
        Func<T, DateTime> endSelector,
        Func<T, int> phaseSelector)
    {
        return source
            .OrderBy(startSelector)
            .GroupBy(phaseSelector)
            .SelectMany(g =>
            {
                var list = g.OrderBy(startSelector).ToList();
                var result = new List<(DateTime, DateTime, List<T>)>();

                DateTime currentStart = startSelector(list[0]).Date;
                DateTime currentEnd = endSelector(list[0]).Date;
                var currentItems = new List<T> { list[0] };

                for (int i = 1; i < list.Count; i++)
                {
                    var item = list[i];
                    var start = startSelector(item).Date;
                    var end = endSelector(item).Date;

                    if (start <= currentEnd.AddDays(1))
                    {
                        if (end > currentEnd) currentEnd = end;
                        currentItems.Add(item);
                    }
                    else
                    {
                        result.Add((currentStart, currentEnd, currentItems));
                        currentStart = start;
                        currentEnd = end;
                        currentItems = new List<T> { item };
                    }
                }

                result.Add((currentStart, currentEnd, currentItems));
                return result;
            })
            .ToList();
    }

    /// <summary>
    /// 翌日有無
    /// </summary>
    /// <typeparam name="T">予定・実績</typeparam>
    /// <param name="date">日付</param>
    /// <param name="dates">日付リスト</param>
    /// <param name="phaseItems">工程リスト</param>
    /// <param name="startSelector">開始日セレクタ</param>
    /// <param name="endSelector">終了日セレクタ</param>
    /// <returns>翌日有無</returns>
    private static bool HasNextDate<T>(
        DateTime date,
        List<DateTime> dates,
        List<T> phaseItems,
        Func<T, DateTime> startSelector,
        Func<T, DateTime> endSelector)
    {
        if (!dates.Any(d => date < d)) return false;

        return phaseItems.Any(a =>
            startSelector(a) <= date.AddDays(2) &&
            date.AddDays(1) <= endSelector(a));
    }

    /// <summary>
    /// セルを描画する
    /// </summary>
    /// <typeparam name="T">データ型</typeparam>
    /// <param name="grouped">グループ化されたデータ</param>
    /// <param name="allItems">すべてのデータ</param>
    /// <param name="phaseSelector">工程ID取得関数</param>
    /// <param name="startSelector">開始日取得関数</param>
    /// <param name="endSelector">終了日取得関数</param>
    /// <param name="idSelector">ID取得関数</param>
    /// <param name="tooltipBaseSelector">ツールチップ用ベース文字列取得関数</param>
    /// <param name="isActual">実績フラグ</param>
    /// <param name="extraAction">追加処理</param>
    private static void RenderCells<T>(
        List<(DateTime start, DateTime end, List<T> items)> grouped,
        List<T> allItems,
        Func<T, int> phaseSelector,
        Func<T, DateTime> startSelector,
        Func<T, DateTime> endSelector,
        Func<T, int> idSelector,
        Func<T, string> tooltipBaseSelector,
        bool isActual,
        List<InputModel> inputModels,
        IEnumerable<DateTime> dates,
        Action<T, InputModel>? extraAction = null)
    {
        // inputModelsをDictionary化して高速化
        var inputModelDict = inputModels
            .ToDictionary(m => (m.MPhaseId, m.IsActual), m => m);

        // phaseごとに事前グループ化
        var itemsByPhase = allItems
            .GroupBy(phaseSelector)
            .ToDictionary(g => g.Key, g => g.ToList());

        var dateList = dates.ToList(); // IEnumerable→Listで高速Any

        foreach (var period in grouped)
        {
            foreach (var item in period.items)
            {
                if (!inputModelDict.TryGetValue((phaseSelector(item), isActual), out var target))
                    continue;

                var phaseItems = itemsByPhase[phaseSelector(item)];

                foreach (var date in dateList)
                {
                    var start = startSelector(item).Date;
                    var end = endSelector(item).Date;

                    if (date < start || date > end) continue;

                    var nextDateExists = HasNextDate(
                        date, dateList, phaseItems, startSelector, endSelector);

                    var tooltip =
                        $"{tooltipBaseSelector(item)} : {period.start:MM/dd}～{period.end:MM/dd}";

                    target.Cells[date] = (
                        idSelector(item),
                        nextDateExists ? "────" : "────‣",
                        tooltip);
                }

                extraAction?.Invoke(item, target);
            }
        }
    }

    /// <summary>
    /// 月を変更する
    /// </summary>
    /// <param name="offset">オフセット。0の場合は当月</param>
    private void ChangeMonth(int offset)
    {
        DateService.ChangeMonth(offset, ref _dateFrom, ref _dateTo);
        LoadSelectedData();
    }

    /// <summary>
    /// Excel保存
    /// </summary>
    /// <returns></returns>
    private async Task DownloadExcelAsync()
    {
        var userName = UserService.GetUserName(UserId);

        using var workbook = new XLWorkbook();

        for (int month = 1; month <= 12; month++)
        {
            GenerateExcel(workbook, month);
        }

        workbook.Worksheet($"{DateFrom.Month}月").SetTabActive();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        using var streamRef = new DotNetStreamReference(stream: ms);

        var fileName = $"{userName}スケジュール.xlsx";
        await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }

    /// <summary>
    /// Excelの１シートを生成
    /// </summary>
    /// <param name="workbook">WorkBook</param>
    /// <param name="month">月（1～12）</param>
    private void GenerateExcel(
        XLWorkbook workbook, int month)
    {
        var (inputModels, dates) = LoadData(
            new DateOnly(DateFrom.Year, month, 1),
            new DateOnly(DateFrom.Year, month,
                DateTime.DaysInMonth(DateFrom.Year, month)));

        if (inputModels == null || inputModels.Count == 0
            || dates == null || !dates!.Any())
        {
            return;
        }

        // 不要行削除
        inputModels = [.. inputModels.Where(i => i.RowClass != "d-none")];

        var grayColor = XLColor.LightGray;
        // 列幅 = センチメートル * 3.31889943
        // 行高さ = センチメートル * 37.7007874015748

        var sheet = workbook.Worksheets.Add($"{month}月");
        sheet.SheetView.ZoomScale = 130;

        // フォント設定
        sheet.Style.Font.FontName = "MS Pゴシック";
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
        var lastRow = inputModels.Count + StartRow - 1;

        sheet.Cell(1, 1).Value = $"{month}月 業務スケジュール";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Cell(1, 1).Style.Fill.BackgroundColor = grayColor;

        sheet.Cell(1, 6).Value = $"{DateFrom.Year}年{month}月";
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
                _ => DbContext.MHoliday
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
            sheet.Cell(row, 3).Value = $"{inputModel.ManHour:#}";
            sheet.Cell(row, 4).Value = inputModel.ProgressRateString;
            sheet.Cell(row, 5).Value = $"{inputModel.EndDate:MM/dd}";

            // 日付ごとの予定・実績
            for (int d = 0; d < dates.Count(); d++)
            {
                var date = dates.ElementAt(d);
                var (id, displayText, _) = inputModel.Cells[date];
                sheet.Cell(row, d + StartColumn).Value = displayText;
            }
        }
    }

    private static string? GetRowClass(InputModel tplanactual)
    {
        return tplanactual.RowClass;
    }
}
