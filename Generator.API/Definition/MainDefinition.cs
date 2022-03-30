namespace Generator.Definition;

public class MainDefinnition : HasYAML
{
    public string? ProjectName { get; set; }
    public string? Description { get; set; }
    public List<string>? Plugins { get; set; }
    public List<FrontendDefinition>? Frontends { get; set; }
    public List<string>? Roles { get; set; }
    public List<EntityDefinition>? Entities { get; set; }
    public List<GatewayDefinition>? Gateways { get; set; }
    public Dictionary<string, string>? Sublogics { get; set; } = new Dictionary<string, string>();
    public List<ComponentDefinition>? Components { get; set; }
}
