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
    private string _projectFilter = string.Empty;
    private string ProjectFilter
    {
        get => _projectFilter;
        set
        {
            if (_projectFilter != value)
            {
                _projectFilter = value;
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
        Logger.LogDebug("▽OnInitializedAsync");
        try
        {
            Users = await UserService.GetUsersAsync();
            UserId = await UserService.GetUserIdAsync(Users, UserId);

            DateService.SetThisMonth(ref _dateFrom, ref _dateTo);
            LoadSelectedData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred!");
        }
        Logger.LogDebug("△OnInitializedAsync");
    }

    /// <summary>
    /// データ読み込み
    /// </summary>
    /// <returns></returns>
    private void LoadSelectedData()
    {
        Logger.LogDebug("▼LoadSelectedData");
        try
        {
            (InputModels, Dates) = LoadData(
                UserService, DbContext, UserId,
                DateFrom, DateTo, ProjectFilter);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred!");
        }
        Logger.LogDebug("▲LoadSelectedData");
    }

    /// <summary>
    /// データ読み込み
    /// </summary>
    /// <param name="dtFrom">開始日</param>
    /// <param name="dtTo">終了日</param>
    /// <param name="projectFilter">プロジェクト名フィルター</param>
    /// <returns></returns>
    private static (List<InputModel>?, IEnumerable<DateTime>?) LoadData(
        UserService userService, ApplicationDbContext dbContext, string userId,
        DateOnly? dtFrom, DateOnly? dtTo, string projectFilter)
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

            var userName = userService.GetUserName(userId);

            dates = Enumerable
                .Range(0, dateTo.DayNumber - dateFrom.DayNumber + 1)
                .Select(offset => dateFrom.AddDays(offset).ToDateTime(
                    TimeOnly.MinValue))
                ;

            var fromDateTime = dateFrom.ToDateTime(TimeOnly.MinValue);
            var toDateTime = dateTo.ToDateTime(TimeOnly.MaxValue);

            // 期間内の工程のプロジェクト
            var projectIds = dbContext.MPhase
                .Where(phase =>
                    dbContext.TPlan.Any(plan =>
                        (string.IsNullOrEmpty(userId) || plan.UserId == userId) &&
                        plan.MPhaseId == phase.Id &&
                        plan.StartDate <= toDateTime &&
                        plan.EndDate >= fromDateTime)
                    ||
                    dbContext.TActual.Any(actual =>
                        (string.IsNullOrEmpty(userId) || actual.UserId == userId) &&
                        actual.MPhaseId == phase.Id &&
                        actual.StartDate <= toDateTime &&
                        actual.EndDate >= fromDateTime)
                )
                .Select(p => p.MProjectId)
                .Distinct();

            // 2カ月以上前のデータは除外
            var twoMonthsAgo = dateFrom.AddMonths(-1);

            // プロジェクトに紐づく工程
            var phases = dbContext.MPhase
                .Where(p =>
                    projectIds.Contains(p.MProjectId) &&
                    (
                        // 予定または実績あり
                        dbContext.TPlan.Any(plan =>
                            (string.IsNullOrEmpty(userId) || plan.UserId == userId) &&
                            plan.MPhaseId == p.Id &&
                            DateOnly.FromDateTime(plan.EndDate) >= twoMonthsAgo)
                        ||
                        dbContext.TActual.Any(actual =>
                            (string.IsNullOrEmpty(userId) || actual.UserId == userId) &&
                            actual.MPhaseId == p.Id &&
                            DateOnly.FromDateTime(actual.EndDate) >= twoMonthsAgo)
                    )
                )
                .Include(p => p.MProject)
                    .ThenInclude(pr => pr.MCustomer)
                .Where(p => p.MProject.Name.Contains(projectFilter))
                .OrderBy(e => e.MProject.MCustomerId)
                .ThenBy(e => e.MProjectId)
                .ThenBy(e => e.Id);

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
            var plans = dbContext.TPlan
                .Where(e => string.IsNullOrEmpty(userId) || e.UserId == userId)
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
            var actuals = dbContext.TActual
                .Where(e => string.IsNullOrEmpty(userId) || e.UserId == userId)
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
            var phaseTotalManHours = dbContext.TActual
                .Where(e => string.IsNullOrEmpty(userId) || e.UserId == userId)
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
            separatorModel2.PhaseName = GetProjectName(separatorModel);
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
                    separatorModel2.PhaseName = GetProjectName(separatorModel);
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
    /// プロジェクト名取得
    /// </summary>
    /// <param name="model"></param>
    /// <returns>顧客名＋プロジェクト名（両方その他の場合はその他）</returns>
    private static string GetProjectName(InputModel model)
    {
        if (model.CustomerName == "その他" && model.ProjectName == "その他")
        {
            return "その他";
        }
        return $"{model.CustomerName} {model.ProjectName}";
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
        return [.. source
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
                        currentItems = [item];
                    }
                }

                result.Add((currentStart, currentEnd, currentItems));
                return result;
            })];
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
        Logger.LogDebug("▽DownloadExcelAsync");
        if (InputModels == null || Dates == null)
        {
            Logger.LogDebug("△DownloadExcelAsync : No data to export.");
            return;
        }

        try
        {
            var userName = UserService.GetUserName(UserId);

            using var workbook = new XLWorkbook();

            using var excelMs = CreateExcelMemoryStream(
                UserService, DbContext, UserId,
                DateFrom.Year, DateFrom.Month, ProjectFilter,
                workbook);
            using var streamRef = new DotNetStreamReference(stream: excelMs);

            var fileName = $"{userName}スケジュール.xlsx";
            await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred!");
        }
        Logger.LogDebug("△DownloadExcelAsync");
    }

    private static string? GetRowClass(InputModel tplanactual)
    {
        return tplanactual.RowClass;
    }
}
