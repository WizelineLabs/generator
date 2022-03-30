namespace Reusable.Attachments;

public interface IAttachment
{
    string AttachmentsFolder { get; set; }

    [Ignore]
    [NotMapped]
    List<Attachment> Attachments { get; set; }
}
