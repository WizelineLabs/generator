using DiffPlex.DiffBuilder.Model;

namespace Generator.API;

public class Diff : BaseEntity
{
    public DiffPane? OldText { get; set; }
    public DiffPane? NewText { get; set; }
}

public class DiffPane
{
    public List<DiffLine>? Lines { get; set; }
}

public class DiffLine
{
    public int Index { get; set; }
    public ChangeType Type { get; set; }
    public int? Position { get; set; }
    public string? Text { get; set; }
    public List<DiffPiece>? SubPieces { get; set; }
    public bool Ignored { get; set; }
}
