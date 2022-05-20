using Generator.API.Generator.SubGenerators;
using Reusable.CRUD.Implementations.EF;
using Reusable.CRUD.JsonEntities;
using Reusable.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Reusable.Rest;
using System.Linq;
using Generator.API.Constants;
namespace Generator.API.Application;

public class ApplicationLogic : WriteLogic<Application>, ILogicWriteAsync<Application>
{
    public string ApplicationsDirectory { get; set; }

    private readonly List<string> SettingsFoldersList = new List<string>()
        {
            SettingsFolders.Frontends,
            SettingsFolders.Dtos,
            SettingsFolders.Entities,
            SettingsFolders.Components
        };

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
        var entitiesPath = definitionPath.CombineWith(SettingsFolders.Entities);
        var dtosPath = definitionPath.CombineWith(SettingsFolders.Dtos); 
        var frontendsPath = definitionPath.CombineWith(SettingsFolders.Frontends);
        var componentsPath = definitionPath.CombineWith(SettingsFolders.Components);

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

    public MainDefinition CreateMainDefinition(Application app)
    {
        Log.Info($"Create MainConfiguration for Application: [{app.Name}]");
        if (string.IsNullOrWhiteSpace(app.Name))
            throw new KnownError("Invalid Application Name");

        if (string.IsNullOrWhiteSpace(ApplicationsDirectory.CombineWith(app.Name)))
            throw new KnownError("Invalid Application Directory Path");

        var mainDefinition = new MainDefinition
        {
            ProjectName = app.Name
        };

        var mainPath = ApplicationsDirectory.CombineWith(app.Name, "definition/main.yml");

        if (File.Exists(mainPath))
            throw new KnownError("Error. Application already exists.");

        var directoryPath = ApplicationsDirectory.CombineWith(app.Name, "definition");
        if (!Directory.Exists(directoryPath))
        {
            Log.Info($"Creating Directories for Definition: [{app.Name}]");
            Directory.CreateDirectory(directoryPath);
            SettingsFoldersList.ForEach(folder =>
                Directory.CreateDirectory(directoryPath.CombineWith(folder))
            );
        }

        File.WriteAllText(mainPath, ParseYML(mainDefinition));
        Log.Info($"Main YAML file written to path: [{mainPath}]");

        return mainDefinition;
    }

    public Application CreateGateway(string name, string entity, string application)
    {
        var gateway = new GatewayDefinition
        {
            Name = name,
            Entity = entity
        };

        return CreateItem(gateway,name, SettingsFolders.Dtos, application, entity);
    }

    public Application CreateEntity(string name, string application)
    {
        var entity = new EntityDefinition
        {
            Name = name
        };

        return CreateItem(entity, name, SettingsFolders.Entities, application);
    }

    public Application CreateComponent(string name, string application)
    {
        var component = new ComponentDefinition
        {
            Name = name,
        };

        return CreateItem(component, name, SettingsFolders.Components, application);
    }

    public Application CreateFrontend(string name, string application)
    {
        var frontend = new FrontendDefinition
        {
            Name = name,
            Pages = new Dictionary<string, string>
                {
                    { "index", null! }
                }
        };

        return CreateItem(frontend, name, SettingsFolders.Frontends, application);
    }

    public Application CreatePage(string pageName, string frontendName, string application)
    {
        Log.Info($"Create Page: [{pageName}] for Application: [{application}] for Frontend: [{frontendName}]");

        var app = GetAll().FirstOrDefault(a => a.Name == application);
        if (app == null)
            throw new KnownError("Application no longer exists.");

        var frontend = app.Definition?.Frontends!.FirstOrDefault(f => f.Name == frontendName);
        if (frontend == null)
            throw new KnownError($"Frontend [{frontendName}] does not exist.");

        frontend.Pages!.Add(pageName, null!);

        var path = ApplicationsDirectory.CombineWith(app.Name!, $"definition/frontends/{frontendName}.yml");

        File.WriteAllText(path, ParseYML(frontend));
        Log.Info($"Page YAML file written: [{path}]");

        //Cache!.FlushAll();

        return GetById(app.Id)!;
    }

    public MainDefinition GetMainDefinition(string appName)
        => GetApp(appName).Definition!;

    public List<ComponentDefinition> GetComponentsInApplication(string appName)    
        => GetApp(appName).Definition!.Components!;
    
    public List<EntityDefinition> GetEntitiesInApplication(string appName)          
        => GetApp(appName).Definition!.Entities!;
    
    public List<GatewayDefinition> GetGatewaysInApplication(string appName) 
        => GetApp(appName).Definition!.Gateways!;
    
    public List<FrontendDefinition> GetFrontendsInApplication(string appName) 
        => GetApp(appName).Definition!.Frontends!;
    
    public Dictionary<string, string> GetPagesInApplicationAndFrontend(string appName, string frontendName)
    {
        var app = GetApp(appName);

        var frontend = app.Definition!.Frontends!.FirstOrDefault(f => f.Name!.ToLower() == frontendName.ToLower().Trim());
        if (frontend == null)
            throw new KnownError($"Frontend [{frontendName}] does not exist.");

        return frontend.Pages!;
    }

    private Application GetApp(string appName)
    {
        var app = GetAll().FirstOrDefault(a => a.Name!.ToLower() == appName.ToLower().Trim());
        if (app == null)
            throw new KnownError($"Application [{appName}] does not exist.");

        AdapterOut(app);

        return app;
    }

    private Application CreateItem<T>(T item, string name, string folder, string application, string entity = "")
    {
        Log.Info($"Create {item!.GetType().Name} : [{name}] for Application: [{application}]");

        var app = GetAll().FirstOrDefault(a => a.Name!.ToLower() == application.ToLower());
        if (app == null)
            throw new KnownError("Application does not exist.");

        var path = ApplicationsDirectory.CombineWith(app.Name!, $"definition/{folder}/{name}.yml");

        var fileInfo = new FileInfo(path);

        if (fileInfo.Exists)
            throw new KnownError("Error. file already exists.");

        if (!fileInfo.Directory!.Exists)
            fileInfo.Directory.Create();

        File.WriteAllText(path, ParseYML(item));
        Log.Info($"YAML file written: [{path}]");

        return GetById(app.Id)!;
    }

    private List<T> getConfigurationFromFolder<T>(string path, Func<T, T> fromYAML)
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

    private string ToYaml(object from)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)          
            .Build();

        return serializer.Serialize(from);        
    }

    private string ParseYML(object from)
    {
        string yaml = ToYaml(from);        
        var lines = yaml.Split(new char[] { '\n' })
            .Where(x => !x.StartsWith("entryState:"))
            .ToList();

        return string.Join('\n', lines);
    }



}