namespace Reusable.Attachments;

[NotMapped]
public class Attachment
{
    public string? AttachmentKind { get; set; }
    public string? FileName { get; set; }
    public string? Comments { get; set; }
    public string? Directory { get; set; }
    public bool ToDelete { get; set; }
}
