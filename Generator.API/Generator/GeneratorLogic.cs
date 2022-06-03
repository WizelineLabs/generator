using Generator.API.BaseGenerators;
using ServiceStack;

namespace Generator.API;

public class GeneratorLogic : WriteLogic<Generator>
{
    public static bool GeneratorIsRunning = false;

    public string? GIT_USER_NAME { get; set; }
    public string? GIT_PASSWORD { get; set; }
    public string? GIT_EMAIL { get; set; }
    public string? GENERATORS_DIRECTORY { get; set; }

    public ApplicationLogic ApplicationLogic { get; set; } = null!;
    // public ConflictLogic ConflictLogic { get; set; }
    // public WorkspaceGenerator WorkspaceGenerator { get; set; }
    // public BackendGenerator BackendGenerator { get; set; }
    public FrontendGenerator FrontendGenerator { get; set; } = null!;
    // public EntityGenerator EntityGenerator { get; set; }
    // public GatewayGenerator GatewayGenerator { get; set; }
    // public ComponentGenerator ComponentGenerator { get; set; } = null!;
    // public PageGenerator PageGenerator { get; set; }

    public GeneratorLogic(
                GeneratorContext DbContext
                , ILog logger
                , IConfiguration configuration
                , ApplicationLogic applicationLogic
                , FrontendGenerator frontendGenerator
                // , ComponentGenerator componentGenerator
    ) : base(DbContext, logger, configuration)
    {
        GIT_USER_NAME = configuration.GetValue<string>("GIT_USERNAME");
        GIT_PASSWORD = configuration.GetValue<string>("GIT_PASSWORD");
        GIT_EMAIL = configuration.GetValue<string>("GIT_EMAIL");
        // GENERATORS_DIRECTORY = AppSettings.Get<string>("generators.directory");
        ApplicationLogic = applicationLogic;
        FrontendGenerator = frontendGenerator;
        // ComponentGenerator = componentGenerator;
    }

    public List<Application> GetApplications()
    {
        return ApplicationLogic.GetAll();
    }

    public MainDefinition? GetApplicationDJSON(string appName)
    {
        var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name!.ToLower() == appName.ToLower().Trim());
        if (app == null)
            throw new KnownError($"Application [{appName}] does not exist.");

        ApplicationLogic.AdapterOut(app);

        return app.Definition;
    }

    public object InspectEntity(string appName, string entityName)
    {
        var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower() == appName.ToLower().Trim());
        if (app == null)
            throw new KnownError($"Application [{appName}] does not exist.");

        ApplicationLogic.AdapterOut(app);

        var entity = app.Definition?.Entities?.FirstOrDefault(e => e.Name.ToLower() == entityName.ToLower());
        if (entity == null)
            throw new KnownError($"Entity: [{entityName}] not found for Application: [{appName}]");

        entity.yaml = null;
        return entity;
    }

    public List<ComponentDefinition>? GetComponentsInApplication(string appName)
    {
        var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower() == appName.ToLower().Trim());
        if (app == null)
            throw new KnownError($"Application [{appName}] does not exist.");

        ApplicationLogic.AdapterOut(app);

        return app.Definition?.Components;
    }

    public List<EntityDefinition>? GetEntitiesInApplication(string appName)
    {
        var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower() == appName.ToLower().Trim());
        if (app == null)
            throw new KnownError($"Application [{appName}] does not exist.");

        ApplicationLogic.AdapterOut(app);

        return app.Definition?.Entities;
    }

    public List<GatewayDefinition>? GetGatewaysInApplication(string appName)
    {
        var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower() == appName.ToLower().Trim());
        if (app == null)
            throw new KnownError($"Application [{appName}] does not exist.");

        ApplicationLogic.AdapterOut(app);

        return app.Definition?.Gateways;
    }

    public List<FrontendDefinition>? GetFrontendsInApplication(string appName)
    {
        var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower() == appName.ToLower().Trim());
        if (app == null)
            throw new KnownError($"Application [{appName}] does not exist.");

        ApplicationLogic.AdapterOut(app);

        return app.Definition?.Frontends;
    }

    public Dictionary<string, string>? GetPagesInApplicationAndFrontend(string appName, string frontendName)
    {
        var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower() == appName.ToLower().Trim());
        if (app == null)
            throw new KnownError($"Application [{appName}] does not exist.");

        ApplicationLogic.AdapterOut(app);

        var frontend = app.Definition?.Frontends?.FirstOrDefault(f => f.Name.ToLower() == frontendName.ToLower().Trim());
        if (frontend == null)
            throw new KnownError($"Frontend [{frontendName}] does not exist.");

        return frontend.Pages;
    }

    public List<ArchiveDTO> Run()
    {
        if (GeneratorIsRunning)
            throw new KnownError("Generator is running already.");

        GeneratorIsRunning = true;

        Log.Info($"Run Generator - All Outdated");

        var files = new List<Archive>();

        try
        {
            // Update Generator Repositories
            // Cache.FlushAll();
            // var generators = GetAll();
            // UpdateGenerator(generators[0]);

            // Update Outdated Apps
            var outdatedApps = ApplicationLogic.GetOutdated();

            foreach (var app in outdatedApps)
                files.AddRange(UpdateApplication(app) as List<Archive>);
        }
        catch (Exception) { throw; }
        finally { GeneratorIsRunning = false; }
        return files.ConvertTo<List<ArchiveDTO>>();
    }

    // public List<ArchiveDTO> RunWorkspace(string? applicationName = null, bool force = false)
    // {
    //     if (string.IsNullOrWhiteSpace(applicationName))
    //     {
    //         Log.Info($"Run Workspace - All Outdated.");

    //         var outdatedApps = ApplicationLogic.GetOutdated();

    //         var files = new List<Archive>();
    //         foreach (var a in outdatedApps)
    //             files.AddRange(UpdateApplicationWorkspace(a, force));

    //         return files.ConvertTo<List<ArchiveDTO>>();
    //     }

    //     Log.Info($"Run Workspace for Application: [{applicationName}]");

    //     var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower() == applicationName.ToLower());
    //     if (app == null)
    //         throw new KnownError($"Application [{applicationName}] does not exist.");

    //     ApplicationLogic.AdapterOut(app);

    //     if (app.Definition == null)
    //         app.Definition = ApplicationLogic.CreateMainDefinition(app);

    //     return UpdateApplicationWorkspace(app, force).ConvertTo<List<ArchiveDTO>>();
    // }

    // public List<ArchiveDTO> RunBackend(string? applicationName = null, bool force = false)
    // {
    //     if (string.IsNullOrWhiteSpace(applicationName))
    //     {
    //         Log.Info($"Run Backend - All Outdated.");

    //         var outdatedApps = ApplicationLogic.GetOutdated();

    //         var files = new List<Archive>();
    //         foreach (var a in outdatedApps)
    //             files.AddRange(UpdateApplicationBackend(a, force));

    //         return files.ConvertTo<List<ArchiveDTO>>();
    //     }

    //     Log.Info($"Run Backend for Application: [{applicationName}]");

    //     var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower() == applicationName.ToLower());
    //     if (app == null)
    //         throw new KnownError($"Application [{applicationName}] does not exist.");

    //     ApplicationLogic.AdapterOut(app);

    //     if (app.Definition == null)
    //         app.Definition = ApplicationLogic.CreateMainDefinition(app);

    //     return UpdateApplicationBackend(app, force).ConvertTo<List<ArchiveDTO>>();
    // }

    public List<ArchiveDTO> RunFrontends(string? applicationName = null, bool force = false)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            Log.Info($"Run Frontend - All Outdated.");

            var outdatedApps = ApplicationLogic.GetOutdated();

            var files = new List<Archive>();
            foreach (var a in outdatedApps)
                files.AddRange(UpdateApplicationFrontends(a, force));

            return files.ConvertTo<List<ArchiveDTO>>();
        }

        Log.Info($"Run Frontend for Application: [{applicationName}]");

        var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower() == applicationName.ToLower());
        if (app == null)
            throw new KnownError($"Application [{applicationName}] does not exist.");

        ApplicationLogic.AdapterOut(app);

        if (app.Definition == null)
            app.Definition = ApplicationLogic.CreateMainDefinition(app);

        return UpdateApplicationFrontends(app, force).ConvertTo<List<ArchiveDTO>>();
    }

    public List<ArchiveDTO> RunApplication(string applicationName, bool force = false)
    {
        Log.Info($"Run Application: [{applicationName}]");

        var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower() == applicationName.ToLower());
        if (app == null)
            throw new KnownError($"Application [{applicationName}] does not exist.");

        ApplicationLogic.AdapterOut(app);

        if (app.Definition == null)
            app.Definition = ApplicationLogic.CreateMainDefinition(app);

        return UpdateApplication(app, force).ConvertTo<List<ArchiveDTO>>();
    }

    // public List<ArchiveDTO> RunComponent(string componentName, string applicationName, bool force = false)
    // {
    //     Log.Info($"Run Component: [{componentName}] of Application: [{applicationName}]");

    //     var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower().Trim() == applicationName.ToLower().Trim());
    //     if (app == null)
    //         throw new KnownError($"Application [{applicationName}] does not exist.");

    //     ApplicationLogic.AdapterOut(app);

    //     if (app.Definition == null)
    //         app.Definition = ApplicationLogic.CreateMainDefinition(app);

    //     var component = app.Definition?.Components?.FirstOrDefault(c => c.Name.ToLower().Trim() == componentName.ToLower().Trim());
    //     if (component == null)
    //         throw new KnownError($"Component [{componentName}] does not exist.");

    //     ComponentGenerator.Setup(app, component);
    //     return ComponentGenerator.Run(force).ConvertTo<List<ArchiveDTO>>();
    // }

    // public List<ArchiveDTO> RunEntity(string entityName, string applicationName, bool force = false)
    // {
    //     Log.Info($"Run Entity: [{entityName}] of Application: [{applicationName}]");

    //     var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower().Trim() == applicationName.ToLower().Trim());
    //     if (app == null)
    //         throw new KnownError($"Application [{applicationName}] does not exist.");

    //     ApplicationLogic.AdapterOut(app);

    //     if (app.Definition == null)
    //         app.Definition = ApplicationLogic.CreateMainDefinition(app);

    //     var entity = app.Definition?.Entities.FirstOrDefault(c => c.Name.ToLower().Trim() == entityName.ToLower().Trim());
    //     if (entity == null)
    //         throw new KnownError($"Entity [{entityName}] does not exist.");

    //     var result = new List<ArchiveDTO>();

    //     EntityGenerator.SetApplication(app);
    //     EntityGenerator.Setup(entity);

    //     result.AddRange(EntityGenerator.Run(force));
    //     result.Add(EntityGenerator.IoCRegistration(app.Definition));
    //     result.Add(EntityGenerator.AddToDBContext(app.Definition));

    //     return result;
    // }

    // public List<ArchiveDTO> RunGateway(string gatewayName, string applicationName, bool force = false)
    // {
    //     Log.Info($"Run Gateway: [{gatewayName}] of Application: [{applicationName}]");

    //     var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower().Trim() == applicationName.ToLower().Trim());
    //     if (app == null)
    //         throw new KnownError($"Application [{applicationName}] does not exist.");

    //     ApplicationLogic.AdapterOut(app);

    //     if (app.Definition == null)
    //         app.Definition = ApplicationLogic.CreateMainDefinition(app);

    //     var gateway = app.Definition?.Gateways.FirstOrDefault(c => c.Name.ToLower().Trim() == gatewayName.ToLower().Trim());
    //     if (gateway == null)
    //         throw new KnownError($"Gateway [{gatewayName}] does not exist.");

    //     var entity = app.Definition?.Entities.FirstOrDefault(c => c.Name.ToLower().Trim() == gateway.Entity.ToLower().Trim());
    //     if (entity == null)
    //         throw new KnownError($"Entity [{gatewayName}] does not exist.");

    //     var result = new List<ArchiveDTO>();

    //     GatewayGenerator.SetApplication(app);
    //     GatewayGenerator.Setup(gateway, entity);

    //     result.AddRange(GatewayGenerator.Run(force));

    //     return result;
    // }

    // public List<ArchiveDTO> RunComponents(string applicationName, string frontendName, bool force = false)
    // {
    //     Log.Info($"Run All Components for Frontend: [{frontendName}] of Application: [{applicationName}]");

    //     var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower().Trim() == applicationName.ToLower().Trim());
    //     if (app == null)
    //         throw new KnownError($"Application [{applicationName}] does not exist.");

    //     ApplicationLogic.AdapterOut(app);

    //     if (app.Definition == null)
    //         app.Definition = ApplicationLogic.CreateMainDefinition(app);

    //     var frontend = app.Definition?.Frontends.FirstOrDefault(c => c.Name.ToLower().Trim() == frontendName.ToLower().Trim());
    //     if (frontend == null)
    //         throw new KnownError($"Frontend [{frontendName}] does not exist.");


    //     return UpdateFrontendComponents(app, frontend, force).ConvertTo<List<ArchiveDTO>>();
    // }

    // public List<ArchiveDTO> RunPages(string applicationName, string frontendName, bool force = false)
    // {
    //     Log.Info($"Run All Pages for Frontend: [{frontendName}] of Application: [{applicationName}]");

    //     var app = ApplicationLogic.GetAll().FirstOrDefault(a => a.Name.ToLower().Trim() == applicationName.ToLower().Trim());
    //     if (app == null)
    //         throw new KnownError($"Application [{applicationName}] does not exist.");

    //     ApplicationLogic.AdapterOut(app);

    //     if (app.Definition == null)
    //         app.Definition = ApplicationLogic.CreateMainDefinition(app);

    //     var frontend = app.Definition?.Frontends.FirstOrDefault(c => c.Name.ToLower().Trim() == frontendName.ToLower().Trim());
    //     if (frontend == null)
    //         throw new KnownError($"Frontend [{frontendName}] does not exist.");


    //     return UpdateFrontendPages(app, frontend, force).ConvertTo<List<ArchiveDTO>>();
    // }

    // public void UpdateGenerator(Generator generator)
    // {
    //     //Log.Info($"Update Generator.");

    //     //if (!string.IsNullOrWhiteSpace(generator.URL))
    //     //{
    //     //    var directory = GENERATORS_DIRECTORY.CombineWith(generator.Name);
    //     //    if (Directory.Exists(directory))
    //     //    {
    //     //        Log.Info($"Clean Generator Directory.");
    //     //        setAttributesNormal(new DirectoryInfo(directory));
    //     //        Directory.Delete(directory, true);
    //     //    }

    //     //    CloneRepository(generator, directory);
    //     //}
    // }

    private void setAttributesNormal(DirectoryInfo dir)
    {
        foreach (var subDir in dir.GetDirectories())
            setAttributesNormal(subDir);
        foreach (var file in dir.GetFiles())
            file.Attributes = FileAttributes.Normal;
    }

    // public void UpdateRepository(Generator generator, string directory)
    // {
    //     Log.Info($"Update Generator Repository.");

    //     string logMessage = "";

    //     FetchOptions options = new FetchOptions();
    //     options.CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
    //         new UsernamePasswordCredentials()
    //         {
    //             Username = GIT_USER_NAME,
    //             Password = GIT_PASSWORD
    //         });

    //     using (var repo = new Repository(directory))
    //     {
    //         var remote = repo.Network.Remotes["origin"];
    //         var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
    //         Commands.Fetch(repo, remote.Name, refSpecs, options, logMessage);
    //         repo.MergeFetchedRefs(new Signature(GIT_USER_NAME, GIT_EMAIL, DateTimeOffset.Now), new MergeOptions
    //         {
    //             FailOnConflict = true
    //         });

    //         Branch branch;
    //         if (!repo.Refs.Any(e => e.CanonicalName == $"refs/heads/{generator.UseBranch}"))
    //             branch = repo.CreateBranch(generator.UseBranch, $"origin/{generator.UseBranch}");
    //         else
    //             branch = repo.Branches.FirstOrDefault(b => b.CanonicalName == $"refs/heads/{generator.UseBranch}");

    //         if (branch != null)
    //         {
    //             Log.Info($"Checkout Generator branch: [{branch}]");
    //             Commands.Checkout(repo, branch);
    //         }

    //     }
    // }

    // public void CloneRepository(Generator generator, string directory)
    // {
    //     Log.Info($"Clone Generator Repository.");

    //     var url = generator.URL;
    //     var result = Repository.Clone(url, directory, new CloneOptions
    //     {
    //         CredentialsProvider = (_url, _user, _cred) =>
    //             new UsernamePasswordCredentials { Username = GIT_USER_NAME, Password = GIT_PASSWORD },
    //         BranchName = generator.UseBranch
    //     });

    //     using (var repo = new Repository(directory))
    //     {
    //         generator.LastCommit = repo.Commits.First().Id.Sha;

    //         Log.Info($"Save Generator Last Commit: [{generator.LastCommit}]");
    //         Update(generator);
    //     }
    // }

    // public List<Archive> UpdateApplicationWorkspace(Application app, bool force = false)
    // {
    //     Log.Info($"Update Application Workspace: [{app.Name}]");

    //     List<Archive> files = new List<Archive>();

    //     WorkspaceGenerator.Setup(app);
    //     files.AddRange(WorkspaceGenerator.Run(force));

    //     return files;
    // }

    // public List<Archive> UpdateApplicationBackend(Application app, bool force = false)
    // {
    //     Log.Info($"Update Application Backend: [{app.Name}]");

    //     List<Archive> files = new List<Archive>();

    //     BackendGenerator.Setup(app);
    //     files.AddRange(BackendGenerator.Run(force));

    //     files.AddRange(UpdateApplicationEntities(app, force));
    //     files.AddRange(UpdateApplicationGateways(app, force));

    //     return files;
    // }

    public List<Archive> UpdateApplicationFrontends(Application app, bool force = false)
    {
        Log.Info($"Update Application Frontends: AppName: [{app.Name}]");

        var definition = ApplicationLogic.GetDefinition(app);
        List<Archive> files = new List<Archive>();

        foreach (var frontend in definition.Frontends)
        {
            FrontendGenerator.Setup(app, frontend);
            files.AddRange(FrontendGenerator.Run(force));

            files.AddRange(UpdateFrontendPages(app, frontend, force));

            //files.AddRange(UpdateFrontendComponents(app, frontend, force));
        }

        return files;
    }

    // public List<Archive> UpdateApplicationEntities(Application app, bool force = false)
    // {
    //     Log.Info($"Update Application Entities: [{app.Name}]");

    //     var definition = ApplicationLogic.GetDefinition(app);
    //     Log.Info($"Application Entities: [{definition.Entities.Select(e => e.Name).Join(", ")}]");

    //     List<Archive> files = new List<Archive>();

    //     EntityGenerator.SetApplication(app);
    //     foreach (var entity in definition.Entities)
    //     {
    //         EntityGenerator.Setup(entity);
    //         files.AddRange(EntityGenerator.Run(force));
    //     }

    //     EntityGenerator.IoCRegistration(definition);
    //     EntityGenerator.AddToDBContext(definition);

    //     return files;
    // }

    // public List<Archive> UpdateApplicationGateways(Application app, bool force = false)
    // {
    //     Log.Info($"Update Application Gateways: [{app.Name}]");

    //     var definition = ApplicationLogic.GetDefinition(app);
    //     Log.Info($"Application Gateways: [{definition.Gateways.Select(e => e.Name).Join(", ")}]");

    //     List<Archive> files = new List<Archive>();

    //     GatewayGenerator.SetApplication(app);
    //     foreach (var gateway in definition.Gateways)
    //     {
    //         var entity = definition.Entities.FirstOrDefault(e => e.Name.ToLower().Trim() == gateway.Entity.ToLower().Trim());
    //         if (entity == null) throw new KnownError($"Entity: {gateway.Entity} does not exist.");
    //         GatewayGenerator.Setup(gateway, entity);
    //         files.AddRange(GatewayGenerator.Run(force));
    //     }

    //     return files;
    // }

    // public List<Archive> UpdateFrontendComponents(Application app, Frontend frontend, bool force = false)
    // {
    //     Log.Info($"Update Application Frontend Components: [{app.Name}]");

    //     var definition = ApplicationLogic.GetDefinition(app);
    //     Log.Info($"Application Components: [{definition.Components.Select(e => e.Name).Join(", ")}]");

    //     List<Archive> files = new List<Archive>();

    //     foreach (var component in definition.Components)
    //     {
    //         ComponentGenerator.Setup(app, component);
    //         files.AddRange(ComponentGenerator.Run(force));
    //     }

    //     return files;
    // }

    public List<Archive> UpdateFrontendPages(Application app, FrontendDefinition frontend, bool force = false)
    {
        Log.Info($"Update Frontend Pages: [{app.Name}], pages: [{frontend.Pages.Select(e => e.Key).Join(", ")}]");

        List<Archive> files = new List<Archive>();

        // PageGenerator.Setup(app, frontend);
        // files.AddRange(PageGenerator.Run(force));

        return files;
    }


    public List<Archive> UpdateApplication(Application app, bool force = false)
    {
        Log.Info($"Update Application: [{app.Name}]");

        List<Archive> files = new List<Archive>();

        // files.AddRange(UpdateApplicationWorkspace(app, force));
        // files.AddRange(UpdateApplicationBackend(app, force));
        files.AddRange(UpdateApplicationFrontends(app, force));

        return files;
    }
}