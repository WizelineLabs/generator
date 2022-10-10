using ServiceStack;

namespace Generator.API;

public class Archive : ArchiveDTO
{
    public Archive()
    {

    }
    public Archive(string projectName,
        string fileType,
        string? frontendName,
        string relativePath,
        string fileName,
        ArchiveComparisionResult ComparisionResult)
    {
        ProjectName = projectName;
        FileType = fileType;
        FrontendName = frontendName;
        RelativePath = relativePath;
        FileName = fileName;
        ComparisionResult.Add(ComparisionResult);
    }

    public string? DirectoryPath { get; set; }
    public string? FullPath { get; set; }
    public new List<ArchiveComparisionResult> ComparisionResult = new List<ArchiveComparisionResult>();
    public string? Content { get; set; }
    public Diff? Diff { get; set; }
}

public class ArchiveDTO
{
    public string? FileName { get; set; }
    public string RelativePath { get; set; } = "";
    public string? FileType { get; set; }
    public List<string> ComparisionResult { get; set; } = new List<string>();
    public string? LeftPath { get; set; }
    public string? RightPath { get; set; }
    public string? Generator { get; set; }
    public string? SubGenerator { get; set; }
    public int DiffIndex { get; set; }
    public string? ProjectName { get; internal set; }
    public string? FrontendName { get; set; }
    public string? ComponentName { get; set; }
}

public enum ArchiveComparisionResult
{
    GeneratorOnly,
    AppOnly,
    Identical,
    Ignore,
    Overwrite,
    Conflict,
    Added,
    Generated,
    Slot
}