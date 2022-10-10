using Reusable.CRUD.JsonEntities;

namespace Generator.API.Generators;

public class EntityGenerator : EntityShared
{
    private Dictionary<string, Archive> Templates = new Dictionary<string, Archive>();
    public bool IsDocument = false;

    public ConflictLogic? ConflictLogic { get; set; }

    public EntityGenerator(GeneratorContext DbContext, ILog logger, IConfiguration configuration, ConflictLogic conflictLogic) : base(DbContext, logger, configuration)
    {
        ConflictLogic = conflictLogic;
    }

    public bool SkipProperties { get; set; }

    public EntityDefinition Parse(EntityDefinition fromYaml)
    {
        return fromYaml;
    }

    public void SetApplication(Application app)
    {
        Application = app;
        MainDefinition = app.Definition;
        Templates = new Dictionary<string, Archive>
            {
                {"entity", new Archive { Content = File.ReadAllText(Configuration.GetValue<string>("ENTITY_TEMPLATES_DIR"))} },
                {"logic", new Archive { Content = File.ReadAllText(Configuration.GetValue<string>("ENTITY_LOGIC_TEMPLATES_DIR"))} }
            };
    }

    public void Setup(EntityDefinition entity)
    {
        Entity = entity;
        IsDocument = entity.Type?.ToLower() == "document";

        Variables = new Dictionary<string, string>
            {
                { "entityName", entity.Name }
            };
    }

    override public List<Archive> Run(bool force = false)
    {
        if (string.IsNullOrWhiteSpace(Entity.Name))
            throw new Exception("Invalid Entity.Name");

        Log.Info($"Running Entity: [{Entity.Name}]");

        var result = new List<Archive>
            {
                GenerateEntity(Entity, force),
                GenerateLogic(Entity, force),
            };

        return result;
    }

    public Archive GetEntityArchive(string toPath, string fileName, string type, ArchiveComparisionResult comparisionResult = ArchiveComparisionResult.Added)
    {
        var f = new Archive(MainDefinition!.ProjectName, type, null, GetRelativePath(toPath, APPLICATIONS_DIRECTORY.CombineWith(Application!.Name), fileName), fileName, comparisionResult);
        f.RightPath = toPath;
        return f;
    }

    public Archive GenerateArchive(EntityDefinition entity, string templateName, string relativePath, string fileName, bool force = false, Dictionary<string, string>? Variables = null)
    {
        var template = Templates[templateName];
        var directoryPath = APPLICATIONS_DIRECTORY.CombineWith(Application!.Name, relativePath, entity.Name);
        var toPath = directoryPath.CombineWith(fileName);

        if (Variables != null)
            foreach (var (key, value) in Variables)
                this.Variables.Add(key, value);

        var content = InterpolateVariables(template.Content!, new Dictionary<string, object> { { "entity", entity } });

        var f = GetEntityArchive(toPath, fileName, ToVariable(templateName));

        var fileInfo = new FileInfo(toPath);
        if (fileInfo.Exists)
        {
            f.Generator = "generator-ssr";

            var diffModel = CompareContent(content, File.ReadAllText(toPath), f, fileInfo.Extension, force);
            f.Diff = ConflictLogic!.ResolveConflicts(diffModel, f, force);
            f.ComparisionResult = f.ComparisionResult.Distinct().ToList();
            WriteFile(f.Content, toPath, fileName);
        }
        else
            WriteFile(content, toPath, fileName, true);

        return f;
    }

    public Archive GenerateEntity(EntityDefinition entity, bool force = false)
    {
        Log.Info($"Generating Entity Model: [{entity.Name}]");

        return GenerateArchive(
            entity,
            "entity",
            "backend/MyApp.API",
            entity.Name + ".cs",
            force,
            new Dictionary<string, string> {
                    { "generated_ctor", GetConstructor(entity) },
                    {"props", GetProperties(entity) }
        });
    }

    public Archive GenerateLogic(EntityDefinition entity, bool force = false)
    {
        Log.Info($"Generating Entity Logic: [{entity.Name}]");

        return GenerateArchive(
            entity,
            "logic",
            "backend/MyApp.API",
            entity.Name + "Logic.cs",
            force);
    }

    public Archive IoCRegistration(MainDefinition djson)
    {
        Log.Info($"IoC Registration for Application: [{Application!.Name}]");

        var toPath = APPLICATIONS_DIRECTORY.CombineWith(Application.Name, "backend/MyApp", "Startup.cs");
        var fileInfo = new FileInfo(toPath);

        InsertGeneratedText(toPath
            , "generated:di"
            , toInsert =>
            {
                djson.Entities.ForEach(e =>
                {
                    toInsert.Add($"container.RegisterAutoWired<{e.Name}Logic>().ReusedWithin(ReuseScope.Request);");
                });
            });

        return GetEntityArchive(fileInfo.FullName, fileInfo.Name, "IoC", ArchiveComparisionResult.Overwrite);
    }

    public Archive AddToDBContext(MainDefinition djson)
    {
        Log.Info($"DBContext for Application: [{Application!.Name}]");

        var toPath = APPLICATIONS_DIRECTORY.CombineWith(Application.Name, "backend/MyApp", "EFContext.cs");
        var fileInfo = new FileInfo(toPath);

        InsertGeneratedText(toPath
            , "generated:dbsets"
            , toInsert =>
            {
                djson.Entities.ForEach(e =>
                {
                    toInsert.Add($"public virtual DbSet<{e.Name}> {e.Name}s {{ get; set; }}");
                });
            });

        return GetEntityArchive(toPath, fileInfo.Name, "EFContext", ArchiveComparisionResult.Overwrite);
    }
}
