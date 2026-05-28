using MailKit.Net.Smtp;
using MimeKit;

namespace WorkOps.Services;

/// <summary>
/// メール送信
/// </summary>
/// <param name="config">環境変数、User Secrets、appsettings.jsonなどの設定</param>
/// <param name="logger">ログ出力用のロガー</param>
public class MailService(IConfiguration config, ILogger<MailService> logger)
{
    /// <summary>
    /// メール送信（ファイル添付あり）
    /// </summary>
    /// <param name="to">送信先メールアドレス</param>
    /// <param name="subject">件名</param>
    /// <param name="textBody">本文</param>
    /// <param name="files">添付ファイルのバイト配列とファイル名のタプルのリスト</param>
    /// <throws="ArgumentNullException">fileBytesがnullの場合</exception>
    /// <throws="InvalidOperationException">SMTPの設定が行われていない場合</exception>
    public async Task SendWithAttachmentAsync(IEnumerable<string> to,
        string subject, string textBody,
        IEnumerable<(byte[] FileBytes, string FileName)> files)
    {
        if (to == null || to.Any() == false)
        {
            throw new ArgumentNullException(nameof(to), "send to is not specified.");
        }
        if (config is null ||
            config["Smtp:Host"] == null ||
            config["Smtp:Port"] == null ||
            config["Smtp:User"] == null ||
            config["Smtp:Password"] == null)
        {
            throw new InvalidOperationException(
                "SMTP settings are not properly configured.");
        }
        logger.LogInformation("Sending email to: {To} subject: {Subject}",
            to, subject);

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("App", config["Smtp:User"]!));
        message.To.AddRange(to.Select(x => MailboxAddress.Parse(x)));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            TextBody = textBody
        };

        foreach (var (fileBytes, fileName) in files)
        {
            var contentType = GetContentType(fileName);

            builder.Attachments.Add(fileName, fileBytes,
                ContentType.Parse(contentType));
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        var useSsl = int.Parse(config["Smtp:Port"]!) == 465;

        await client.ConnectAsync(
            config["Smtp:Host"]!,
            int.Parse(config["Smtp:Port"]!),
            useSsl);

        await client.AuthenticateAsync(
            config["Smtp:User"]!,
            config["Smtp:Password"]!);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    /// <summary>
    /// Content-Typeをファイル名から推測して返す
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <returns>Content-Type</returns>
    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        return ext switch
        {
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            _ => "application/octet-stream"
        };
    }
}
