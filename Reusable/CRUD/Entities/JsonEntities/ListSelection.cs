namespace Reusable.CRUD.JsonEntities;

public enum SelectionState
{
    NULL,
    NONE,
    ALL
}

public class ListSelection
{
    public long[]? Selected { get; set; }
    public long[]? Unselected { get; set; }
    public bool IsAllSelected { get; set; }
    public bool IsAllUnselected { get; set; }
    public SelectionState SelectionState { get; set; }
}
