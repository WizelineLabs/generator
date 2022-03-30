namespace Generator.Definition;

public class GatewayDefinition : HasYAML
{
    public string? Name { get; set; }
    public string? Entity { get; set; }
    public List<Dto> Dtos { get; set; } = new List<Dto>();

    public EntryState EntryState { get; set; }
}

public class Dto
{
    public string? Name { get; set; } // Name is taken from Dictionary Key.
    public string? Def { get; set; }
    public List<DtoRoute> Routes { get; set; } = new List<DtoRoute>();
    public Dictionary<string, string> Request { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> Response { get; set; } = new Dictionary<string, string>();

}

public class DtoRoute
{
    public string? HttpVerb { get; set; }
    public string? Path { get; set; }
}