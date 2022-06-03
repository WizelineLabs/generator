namespace Generator.Definition;

public class FrontendDefinition : HasYAML
{
    public string Name { get; set; } = null!;
    public string? DisplayName { get; set; }
    public Dictionary<string, string> Pages { get; set; } = new Dictionary<string, string>();

    public EntryState EntryState { get; set; }
}
