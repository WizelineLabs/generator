using System.Runtime.Serialization;

namespace Generator.Definition;

[DataContract]
public class ComponentDefinition : HasYAML
{
    [DataMember(Order = 0)]
    public string Name { get; set; } = null!;
    
    [DataMember(Order = 1)]
    public string? Type { get; set; }
    
    [DataMember(Order = 2)]
    public string? Entity { get; set; }
    
    [DataMember(Order = 3)]
    public List<object> Layout = new List<object>();
    
    [DataMember(Order = 4)]
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
