using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using WorkOps.Utils;

namespace WorkOps.Components.Pages.TReportPages;

/// <summary>
/// Word出力処理部分
/// </summary>
public partial class Index
{
    /// <summary>
    /// WordのMemoryStreamを生成
    /// </summary>
    /// <param name="userName">ユーザー名</param>
    /// <returns>生成されたMemoryStream</returns>
    private async Task<MemoryStream> CreateWordMemoryStreamAsync(string userName)
    {
        var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(
            ms, WordprocessingDocumentType.Document))
        {
            // Add a main document part. 
            MainDocumentPart mainPart = doc.AddMainDocumentPart();

            // Create the document structure and add some text.
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());

            foreach (var (id, index)
                in InputModels!.Where(x => x.Id != 0).Select((x, i) => (x.Id, i)))
            {
                var inputModel = await LoadDataDetailsAsync(id);
                if (inputModel is null)
                {
                    continue;
                }
                var indexNo = StringUtil.ConvertDigitsToFullWidth(index);

                List<string> headerTexts = [
                    $"{AppSettings.Value.CompanyName}　週間報告書　{Month:MM}月－{indexNo}",
                            $"氏名：{userName}　期間：{inputModel.Date:yyyy/MM/dd} ～ {inputModel.Date.AddDays(6):yyyy/MM/dd}"
                ];

                var bodyParagraphs = CreateBodyParagraph(inputModel);

                AddSection(mainPart, body, headerTexts, bodyParagraphs,
                    index != InputModels!.Count(x => x.Id != 0) - 1);
            }
        }

        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// セクション追加
    /// </summary>
    /// <param name="mainPart">メイン文書部分</param>
    /// <param name="body">本文</param>
    /// <param name="headerTexts">ヘッダー文言</param>
    /// <param name="bodyParagraphs">本文</param>
    /// <param name="isBreak">改ページ</param>
    static void AddSection(MainDocumentPart mainPart, Body body,
        List<string> headerTexts, List<Paragraph> bodyParagraphs, bool isBreak)
    {
        // 1. ヘッダーパーツの作成とテキスト設定
        HeaderPart headerPart = mainPart.AddNewPart<HeaderPart>();
        string headerId = mainPart.GetIdOfPart(headerPart);
        headerPart.Header = new Header(
            headerTexts.Select(t => new Paragraph(new Run(new Text(t))))
        );

        // 2. 本文の追加
        foreach (var bodyParagraph in bodyParagraphs)
        {
            body.AppendChild(bodyParagraph);
        }

        // 3. セクション設定の構築
        SectionProperties secProps = new SectionProperties();
        secProps.AppendChild(new HeaderReference()
        {
            Type = HeaderFooterValues.Default,
            Id = headerId
        });

        if (isBreak)
        {
            // 「次のページから開始」を設定（改ページ）
            secProps.AppendChild(new SectionType()
            {
                Val = SectionMarkValues.NextPage
            });

            // 文中のセクション区切りは、段落のプロパティ（ParagraphProperties）に含める
            Paragraph lastPara = new Paragraph();
            lastPara.AppendChild(new ParagraphProperties(secProps));
            body.AppendChild(lastPara);
        }
        else
        {
            // 最終セクションはBodyの直下に追加する
            body.AppendChild(secProps);
        }
    }

    /// <summary>
    /// 本文作成
    /// </summary>
    /// <param name="inputModel">入力モデル</param>
    /// <returns>本文</returns>
    private List<Paragraph> CreateBodyParagraph(InputModel inputModel)
    {
        var paragraphs = new List<Paragraph>();

        if (inputModel is null || inputModel.InputDetailModels is null
            || inputModel.InputDetailModels.Count == 0)
        {
            return paragraphs;
        }

        foreach (var (inputDetailModel, index) in inputModel.InputDetailModels
            .Select((x, i) => (x, i)))
        {
            var indexNo = StringUtil.ConvertDigitsToFullWidth(index);
            AddLines($"{indexNo}　{inputDetailModel.ProjectName}", paragraphs, false);
            AddLines($"{indexNo}－１　作業内容", paragraphs, false);
            AddLines(inputDetailModel.Description, paragraphs, true);
            AddLines($"", paragraphs, false);
            AddLines($"{indexNo}－２　課題・問題", paragraphs, false);
            AddLines($"{inputDetailModel.Problem}", paragraphs, true);
            AddLines($"", paragraphs, false);
            AddLines($"{indexNo}－３　今後の予定", paragraphs, false);
            AddLines($"{inputDetailModel.Schedule}", paragraphs, true);
        }
        AddLines($"", paragraphs, false);
        AddLines($"◆残業時間", paragraphs, false);
        AddLines($"1. 合計 0H", paragraphs, true);

        return paragraphs;
    }

    /// <summary>
    /// 改行で分割して追加
    /// </summary>
    /// <param name="text">テキスト</param>
    /// <param name="paragraphs">章</param>
    /// <param name="isIndent">インデントあり</param>
    private void AddLines(string text, List<Paragraph> paragraphs,
        bool isIndent)
    {
        var lines = text.Split('\n').Select(line =>
            new Paragraph(
                new ParagraphProperties(
                    new Indentation() { Left = isIndent ? "360" : "0" }
                ),
                new Run(
                    new Text($"{line}")
            )));
        foreach (var line in lines)
        {
            paragraphs.Add(line);
        }
    }
}
