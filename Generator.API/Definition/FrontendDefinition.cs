namespace Generator.Definition;

public class FrontendDefinition : HasYAML
{
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public Dictionary<string, string>? Pages { get; set; }

    public EntryState EntryState { get; set; }
}
