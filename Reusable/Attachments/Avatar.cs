namespace Reusable.Attachments;

public interface IAvatar
{
    string AvatarFolder { get; set; }

    [Ignore]
    [NotMapped]
    List<Avatar> AvatarList { get; set; }
}

[NotMapped]
public class Avatar : Attachment
{
    public string? ImageBase64 { get; set; }
}
