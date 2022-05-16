using Generator.API.Generator.SubGenerators;
using Reusable.CRUD.Implementations.EF;
using Reusable.CRUD.JsonEntities;
using Reusable.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Generator.API.Application;

public class ApplicationLogic : WriteLogic<Application>, ILogicWriteAsync<Application>
{
    public string ApplicationsDirectory { get; set; }

    public ApplicationLogic(GeneratorContext DbContext, Log<ApplicationLogic> logger, IConfiguration configuration) : base(DbContext, logger, configuration)
    {
        ApplicationsDirectory = configuration.GetValue<string>("ConfigurationFolderPath");
    }

    public override List<Application> AdapterOut(params Application[] entities)
    {
        foreach (var item in entities)
            item.Definition = GetDefinition(item);

        return entities.ToList();
    }

    public MainDefinition GetDefinition(Application app)
    {
        return GetDefinition(Path.Combine(ApplicationsDirectory, app.Name!, "definition"));
    }

    public MainDefinition GetDefinition(string definitionPath)
    {
        MainDefinition? mainDefinition = null;
        var mainPath = definitionPath.CombineWith("main.yml"); 
        var entitiesPath = definitionPath.CombineWith("entities");
        var dtosPath = definitionPath.CombineWith("dtos"); 
        var frontendsPath = definitionPath.CombineWith("frontends");
        var componentsPath = definitionPath.CombineWith("components");

        if (File.Exists(mainPath))
        {
            mainDefinition = DeserializeYAML<MainDefinition>(mainPath);            
            mainDefinition.Frontends = getConfigurationFromFolder<FrontendDefinition>(frontendsPath, new FrontendGenerator().Parse);
            mainDefinition.Entities = getConfigurationFromFolder<EntityDefinition>(entitiesPath, new EntityGenerator().Parse);
            mainDefinition.Components = getConfigurationFromFolder<ComponentDefinition>(componentsPath, new ComponentGenerator().Parse);
            mainDefinition.Gateways = getConfigurationFromFolder<GatewayDefinition>(dtosPath, new GatewayGenerator().Parse);            
        }

        return mainDefinition!;
    }

    public List<T> getConfigurationFromFolder<T>(string path, Func<T,T> fromYAML)
    {
        List<T> results = new List<T>();
        if (Directory.Exists(path))
        {            
            string[] fileEntries = Directory.GetFiles(path, "*.yml");
            foreach (string file in fileEntries)
            {
                T toAdd = fromYAML(DeserializeYAML<T>(file));
                results.Add(toAdd);
            }            
        }
        return results;
    }

    private T DeserializeYAML<T>(string yamlName)
    {
        using (StreamReader sr = new StreamReader(yamlName))
        {
            var input = new StringReader(sr.ReadToEnd());
            var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
            return deserializer.Deserialize<T>(input);
        }
    }
}