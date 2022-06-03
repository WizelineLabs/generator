namespace Generator.Definition;

public class ComponentDefinition : HasYAML
{
    public string Name { get; set; } = null!;
    public string? Type { get; set; }
    public string? Entity { get; set; }
    public List<object> Layout = new List<object>();
    public Dictionary<object, object> Slots = new Dictionary<object, object>();

    public EntryState EntryState { get; set; }
}

public class ComponentSlots
{
    public string? Dependencies { get; set; }
    public string? Config { get; set; }
    public string? Interface { get; set; }
    public string? Ctor { get; set; }
    public string? Load { get; set; }
    public string? AfterLoad { get; set; }
    public string? AfterCreate { get; set; }
    public string? AfterSave { get; set; }
    public string? BeforeSave { get; set; }
    public string? OnChange { get; set; }
    public string? Body { get; set; }
    public string? Render { get; set; }
    public string? AfterRemove { get; set; }
    public string? OnOpenItem { get; set; }
}
