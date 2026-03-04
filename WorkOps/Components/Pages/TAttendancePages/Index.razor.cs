using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using WorkOps.Data;
using WorkOps.Models.Attributes;
using WorkOps.Services;
using WorkOps.Utils;

namespace WorkOps.Components.Pages.TAttendancePages;

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
#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                LoadSelectedDataAsync();
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
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
#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                LoadSelectedDataAsync();
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
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
#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
                LoadSelectedDataAsync();
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
            }
        }
    }
    private List<InputModel>? InputModels;
    private readonly PaginationState pagination = new() { ItemsPerPage = 31 };
    private List<ApplicationUser> Users = [];

    // 合計有給時間
    private string totalPaidLeave = string.Empty;
    // 合計勤務時間
    private string totalWorked = string.Empty;
    // 合計時間外
    private string totalOvertime = string.Empty;
    // 合計時間
    private string totalTime = string.Empty;
    // 稼働日
    private string workingDays = "0";

    /// <summary>
    /// 管理者権限なし
    /// </summary>
    private bool hassNotAuthorized = true;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        Users = await UserService.GetUsersAsync();
        UserId = await UserService.GetUserIdAsync(Users, UserId);

        // 管理者の場合承認可
        hassNotAuthorized = !await UserService.HasAdminRoleAsync();

        DateService.SetThisMonth(ref _dateFrom, ref _dateTo);
        await LoadSelectedDataAsync();
    }

    /// <summary>
    /// データ読み込み
    /// </summary>
    /// <returns></returns>
    private async Task LoadSelectedDataAsync()
    {
        InputModels = await LoadDataAsync(DateFrom, DateTo);
    }

    /// <summary>
    /// データ読み込み
    /// </summary>
    /// <param name="dtFrom">開始日</param>
    /// <param name="dtTo">終了日</param>
    /// <returns></returns>
    private async Task<List<InputModel>?> LoadDataAsync(
        DateOnly? dtFrom, DateOnly? dtTo)
    {
        List<InputModel> inputModels = [];
        try
        {
            if (dtFrom == default || dtTo == default
                || dtFrom == DateOnly.MinValue || dtTo == DateOnly.MinValue)
            {
                return inputModels;
            }

            var tattendances = DbContext.TAttendance
                .Where(ta => ta.UserId == UserId
                    && dtFrom <= ta.Date && ta.Date <= dtTo);
            var models = tattendances.AsEnumerable()
                .Select(e =>
                {
                    var inputModel = new InputModel();
                    PropertyUtil.CopyProperties(e, inputModel);
                    return inputModel;
                })
                .ToList()
                ;
            foreach (var model in models)
            {
                PropertyUtil.CopyProperties(
                    tattendances.Where(e => e.Id == model.Id).FirstOrDefault(),
                    model);
            }

            // 作業内容
            var tactuals = DbContext.TActual
                .Include(e => e.MPhase)
                .Include(e => e.MPhase.MProject)
                .Include(e => e.MPhase.MProject.MCustomer)
                .Where(e => e.UserId == UserId
                    && dtFrom <= DateOnly.FromDateTime(e.EndDate)
                    && DateOnly.FromDateTime(e.StartDate) <= dtTo
                )
                .AsEnumerable();
            // AM
            var tactualsAm = tactuals
                .Where(e => e.StartDate.TimeOfDay <= new TimeSpan(12, 0, 0))
                .SelectMany(ta =>
                    Enumerable.Range(0, (ta.EndDate - ta.StartDate).Days + 1)
                    .Select(offset => new
                    {
                        tactual = ta,
                        Date = DateOnly.FromDateTime(ta.StartDate.AddDays(offset))
                    })
                )
            ;
            // PM
            var tactualsPm = tactuals
                .Where(e => new TimeSpan(12, 0, 0) < e.EndDate.TimeOfDay)
                .SelectMany(ta =>
                    Enumerable.Range(0, (ta.EndDate - ta.StartDate).Days + 1)
                    .Select(offset => new
                    {
                        tactual = ta,
                        Date = DateOnly.FromDateTime(ta.StartDate.AddDays(offset))
                    })
                )
            ;

            var entities = DbContext.TAttendance
                .Include(e => e.User)
                .Include(e => e.User!.MWorkTime)
                .AsEnumerable();

            var attendances = entities
                .Join(
                    models,
                    entity => entity.Id,
                    inputModel => inputModel.Id,
                    (entity, inputModel) => new
                    {
                        entity,
                        inputModel,
                    })
                .GroupJoin(
                    DbContext.MHoliday,
                    models => models.entity.Date,
                    entity => entity.Date,
                    (models, entity) => new
                    {
                        models.entity,
                        models.inputModel,
                        HolidayName = entity.FirstOrDefault()?.Name,
                    })
                .GroupJoin(
                    tactualsAm,
                    attendance => new
                    {
                        attendance.entity.UserId,
                        attendance.entity.Date
                    },
                    tactualsAm => new
                    {
                        tactualsAm.tactual.UserId,
                        tactualsAm.Date
                    },
                    (attendance, tactualsAm) => new { attendance, tactualsAm }
                )
                .GroupJoin(
                    tactualsPm,
                    atteAndAm => new
                    {
                        atteAndAm.attendance.entity.UserId,
                        atteAndAm.attendance.entity.Date
                    },
                    tactualsPm => new
                    {
                        tactualsPm.tactual.UserId,
                        tactualsPm.Date
                    },
                    (atteAndAm, tactualsPm) =>
                    {
                        var inputModel = new InputModel
                        {
                            UserName = atteAndAm.attendance.entity.User?.FullName ?? string.Empty,
                            HolidayName = atteAndAm.attendance.HolidayName,
                            PaidLeaveDurationString = GetDulationString(
                                atteAndAm.attendance.entity.Date,
                                atteAndAm.attendance.entity.PaidLeaveDuration),
                            WorkedDurationString = GetDulationString(
                                atteAndAm.attendance.entity.Date,
                                atteAndAm.attendance.entity.WorkedDuration),
                            OvertimeDurationString = GetDulationString(
                                atteAndAm.attendance.entity.Date,
                                atteAndAm.attendance.entity.OvertimeDuration),
                            WorkDetailAm = string.Join(",", atteAndAm.tactualsAm
                                .Select(a =>
                                $"{a.tactual.MPhase.MProject.Name} {a.tactual.MPhase.Name}")
                                .ToList()),
                            WorkDetailPm = string.Join(",", tactualsPm
                                .Select(a =>
                                $"{a.tactual.MPhase.MProject.Name} {a.tactual.MPhase.Name}")
                                .ToList()),
                        };
                        atteAndAm.attendance.inputModel.PaidLeaveDurationString
                            = inputModel.PaidLeaveDurationString;
                        atteAndAm.attendance.inputModel.WorkedDurationString
                            = inputModel.WorkedDurationString;
                        atteAndAm.attendance.inputModel.OvertimeDurationString
                            = inputModel.OvertimeDurationString;
                        atteAndAm.attendance.inputModel.WorkDetailAm
                            = inputModel.WorkDetailAm;
                        atteAndAm.attendance.inputModel.WorkDetailPm
                            = inputModel.WorkDetailPm;

                        PropertyUtil.CopyProperties(
                            atteAndAm.attendance.entity, inputModel);
                        PropertyUtil.CopyProperties(
                            atteAndAm.attendance.inputModel, inputModel);
                        return inputModel;
                    }
            );

            for (var day = (dtFrom ?? DateOnly.MinValue);
                day <= (dtTo ?? DateOnly.MinValue); day = day.AddDays(1))
            {
                inputModels.Add(
                attendances
                .FirstOrDefault(ia => ia.Date == day) ?? new InputModel
                {
                    UserId = UserId,
                    UserName = Users
                        .FirstOrDefault(u => u.Id == UserId)?
                        .FullName ?? string.Empty,
                    Date = day,
                    HolidayName = DbContext.MHoliday
                        .FirstOrDefault(h => h.Date == day)?
                        .Name ?? string.Empty,
                });
            }

            // 合計値算出
            totalPaidLeave = GettotalTime(
                attendances, e => e.PaidLeaveDuration);
            totalWorked = GettotalTime(
                attendances, e => e.WorkedDuration);
            totalOvertime = GettotalTime(
                attendances, e => e.OvertimeDuration);
            // 稼働日
            var days = attendances.Count(e => e.StartTime != null);
            workingDays = $"{days}";
            // 合計時間
            totalTime = $"{days * 8}時間";

            return inputModels;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return inputModels;
        }
    }

    /// <summary>
    /// 行クラス取得
    /// 土日の場合のCSSクラス設定
    /// </summary>
    /// <param name="tattendance">行</param>
    /// <returns>行クラス</returns>
    private static string? GetRowClass(InputModel tattendance)
    {
        var dow = tattendance.Date.DayOfWeek;

        return dow switch
        {
            DayOfWeek.Saturday => "saturday",
            DayOfWeek.Sunday => "sunday",
            _ => string.IsNullOrEmpty(tattendance.HolidayName) ? null : "holiday"
        };
    }

    /// <summary>
    /// 月を変更する
    /// </summary>
    /// <param name="offset">オフセット。0の場合は当月</param>
    private async Task ChangeMonthAsync(int offset)
    {
        DateService.ChangeMonth(offset, ref _dateFrom, ref _dateTo);
        await LoadSelectedDataAsync();
    }

    /// <summary>
    /// 更新または新規作成画面遷移URL
    /// </summary>
    /// <param name="tattendance">行</param>
    /// <returns>更新または新規作成画面遷移URL</returns>
    private string GetEditOrCreateUrl(InputModel tattendance)
    {
        if (tattendance.Id == 0)
        {
            return "tattendances/create?userid=" + UserId
            + "&date=" + tattendance.Date;
        }
        return "tattendances/edit?id=" + tattendance.Id;
    }

    /// <summary>
    /// TimeSpanをDateTimeに変換
    /// </summary>
    /// <param name="date"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    private static string GetDulationString(DateOnly date, TimeSpan? timeSpan)
    {
        if (timeSpan == null || timeSpan == TimeSpan.Zero)
        {
            return string.Empty;
        }
        return date
            .ToDateTime(TimeOnly.MinValue)
            .Add(timeSpan ?? TimeSpan.Zero)
            .ToString("HH:mm");
    }

    /// <summary>
    /// TimeSpanの合計をDateTimeで返す
    /// </summary>
    /// <param name="entities">合計する列を含むエンティティ</param>
    /// <param name="selector">合計する列のセレクタ</param>
    /// <returns></returns>
    private static string GettotalTime<T>(
        IEnumerable<T> entities, Func<T, TimeSpan?> selector)
    {
        var total = entities
            .Select(selector)
            .Aggregate(TimeSpan.Zero, (sum, d) => sum + (d ?? TimeSpan.Zero));

        return $"{(int)total.TotalHours:00}:{total.Minutes:00}";
    }

    /// <summary>
    /// Excel保存
    /// </summary>
    /// <returns></returns>
    private async Task DownloadExcelAsync()
    {
        var userName = InputModels?.FirstOrDefault()?.UserName;

        using var workbook = new XLWorkbook();

        for (int month = 1; month <= 12; month++)
        {
            await GenerateExcelAsync(workbook, month, userName ?? string.Empty);
        }

        workbook.Worksheet($"{DateFrom.Month}月").SetTabActive();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        using var streamRef = new DotNetStreamReference(stream: ms);

        var fileName = $"{DateFrom:yyyy}年勤務表_{userName}.xlsx";
        await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
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
                StartTimeString = im.StartTime?.ToString("HH:mm"),
                EndTimeString = im.EndTime?.ToString("HH:mm"),
                PaidLeaveDurationString = im.PaidLeaveDurationString,
                WorkedDurationString = im.WorkedDurationString,
                OvertimeDurationString = im.OvertimeDurationString,
                Remarks = im.Remarks,
                WorkDetailAm = im.WorkDetailAm,
                WorkDetailPm = im.WorkDetailPm,
            });

        var sheet = workbook.Worksheets.Add($"{month}月");
        sheet.SheetView.ZoomScale = 70;

        // フォント設定
        sheet.Style.Font.FontName = "MS Pゴシック";
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
        var lastRow = startRow + excelModels!.Count() - 1;
        var lastCol = startCol + typeof(ExcelModel).GetProperties().Length - 1;

        // 合計
        sheet.Range(lastRow + 1, 1, lastRow + 1, 3).Merge();
        sheet.Cell(lastRow + 1, 1).Value = "合計";
        sheet.Cell(lastRow + 1, 6).Value = totalPaidLeave;
        sheet.Cell(lastRow + 1, 7).Value = totalWorked;
        sheet.Cell(lastRow + 1, 8).Value = totalOvertime;
        sheet.Cell(lastRow + 2, 3).Value = totalTime;
        sheet.Cell(lastRow + 2, 5).Value = "稼働日";
        sheet.Cell(lastRow + 2, 7).Value = workingDays;

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
        sheet.Cell(lastRow + 2, 3).Style.Alignment.Horizontal
            = XLAlignmentHorizontalValues.Center;
        sheet.Cell(lastRow + 2, 7).Style.Alignment.Horizontal
            = XLAlignmentHorizontalValues.Right;

        // 印刷範囲
        sheet.PageSetup.PrintAreas.Add("A1:I35");
        sheet.PageSetup.FitToPages(1, 1);

        // 余白の設定
        sheet.PageSetup.Margins.Top = 2;
        sheet.PageSetup.Margins.Bottom = 1;
        sheet.PageSetup.Margins.Left = 1.5;
        sheet.PageSetup.Margins.Right = 1;
        sheet.PageSetup.Margins.Header = 1.3;
        sheet.PageSetup.Margins.Footer = 1.3;
    }
}
