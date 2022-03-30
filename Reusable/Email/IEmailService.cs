namespace Reusable.EmailServices;

public interface IEmailService
{
    string? From { get; set; }
    string? FromPassword { get; set; }

    List<string> To { get; set; }
    List<string> Cc { get; set; }
    List<string> Bcc { get; set; }
    string? Template { get; set; }
    Dictionary<string, object> TemplateParameters { get; set; }

    string? Subject { get; set; }
    string? Body { get; set; }

    string? AttachmentsFolder { get; set; }

    Task SendMail();
}

