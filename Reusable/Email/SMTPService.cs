namespace Reusable.EmailServices;

using Reusable.Attachments;
using Reusable.Rest;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;

public class SMTPService : IEmailService
{
    public static ILog Log = LogManager.GetLogger("MyApp");

    public static IAppSettings? AppSettings { get; set; }
    private SmtpClient? smtp;

    public string? From { get; set; }
    public string? FromPassword { get; set; }

    public List<string> To { get; set; } = new List<string>();
    public List<string> Cc { get; set; } = new List<string>();
    public List<string> Bcc { get; set; } = new List<string>();

    public string? Subject { get; set; }
    public string? Body { get; set; }

    public string? AttachmentsFolder { get; set; }
    public string? Template { get; set; }
    public Dictionary<string, object> TemplateParameters { get; set; } = new Dictionary<string, object>();

    public async Task SendMail()
    {
        From = AppSettings!.Get<string>("EMAIL_FROM");
        FromPassword = AppSettings.Get<string>("SMTP_PASSWORD");

        var smtpServer = AppSettings.Get<string>("SMTP_SERVER");
        var smtpPort = AppSettings.Get("SMTP_PORT", 25);
        var smtpSSL = AppSettings.Get("SMTP_SSL", true);

        if (string.IsNullOrWhiteSpace(smtpServer) || smtpPort <= 0)
            throw new KnownError("Invalid SMTP settings.");

        smtp = new SmtpClient(smtpServer, smtpPort);

        smtp.EnableSsl = smtpSSL;
        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtp.UseDefaultCredentials = false;
        smtp.Credentials = new System.Net.NetworkCredential(From, FromPassword);

        MailMessage message = new MailMessage();
        message.From = new MailAddress(From, From);

        foreach (var to in To)
        {
            message.To.Add(new MailAddress(to));
        }
        foreach (var cc in Cc)
        {
            message.CC.Add(new MailAddress(cc));
        }
        foreach (var bcc in Bcc)
        {
            message.Bcc.Add(new MailAddress(bcc));
        }

        message.Subject = Subject;
        message.IsBodyHtml = true;
        message.BodyEncoding = System.Text.Encoding.UTF8;

        message.Body = Body;

        string baseAttachmentsPath = AppSettings.Get<string>("EmailAttachments");
        var attachments = AttachmentsFolder != null ? AttachmentsIO.getAttachmentsFromFolder(AttachmentsFolder, "EmailAttachments") : new List<Attachments.Attachment>();
        foreach (var attachment in attachments)
        {
            string filePath = baseAttachmentsPath + attachment.Directory + "\\" + attachment.FileName;
            FileInfo file = new FileInfo(filePath);
            message.Attachments.Add(new System.Net.Mail.Attachment(new FileStream(filePath, FileMode.Open, FileAccess.Read), attachment.FileName));
        }

        await smtp.SendMailAsync(message);

        if (Log.IsDebugEnabled)
            Log.Info($"SMTP Email sent by: [{From}] to:[{To.Join(", ")}] cc:[{Cc.Join(", ")}] bcc:[{Bcc.Join(", ")}] subject: [{Subject}]");
    }
}
