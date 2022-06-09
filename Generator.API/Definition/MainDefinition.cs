namespace Generator.Definition;

public class MainDefinition : HasYAML
{
    public string ProjectName { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Plugins { get; set; } = new List<string>();
    public List<FrontendDefinition> Frontends { get; set; } = new List<FrontendDefinition>();
    public List<string> Roles { get; set; } = new List<string>();
    public List<EntityDefinition> Entities { get; set; } = new List<EntityDefinition>();
    public List<GatewayDefinition> Gateways { get; set; } = new List<GatewayDefinition>();
    public Dictionary<string, string> Sublogics { get; set; } = new Dictionary<string, string>();
    public List<ComponentDefinition> Components { get; set; } = new List<ComponentDefinition>();
}
