using Reusable.CRUD.Implementations.EF;
using Reusable.Utils;

namespace Generator.API.Application;

public class ApplicationLogic : WriteLogic<Application>, ILogicWriteAsync<Application>
{
    public string ApplicationsDirectory { get; set; }

    public ApplicationLogic(GeneratorContext DbContext, Log<ApplicationLogic> logger, IConfiguration configuration) : base(DbContext, logger, configuration)
    {
        ApplicationsDirectory = configuration["applications.directory"];
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
        return new MainDefinition();
    }
}