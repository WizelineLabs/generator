
namespace Generator.API;
public class Conflict : Trackable
{
    public string? Generator { get; set; }
    public string? SubGenerator { get; set; }
    public string? Application { get; set; }
    public string? GeneratorSource { get; set; }
    public string? ApplicationSource { get; set; }
    public string? RelativePath { get; set; }
    public string? FileName { get; set; }
    public string? DiffModel { get; set; }
    public ConflictResolution Resolution { get; set; }
    public string? ChangeType { get; set; }
    public int? Position { get; set; }
    public int DiffIndex { get; set; }
    public string? LineBefore { get; set; }
    public string? LineAfter { get; set; }
    public enum ConflictResolution
    {
        IgnoreApp,
        IgnoreAll,
        OverwriteApp,
        OverwriteAll,
        AskUser
    }
}