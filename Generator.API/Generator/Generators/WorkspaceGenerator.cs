using Generator.API.BaseGenerators;
using ServiceStack;


namespace Generator.API.Generators
{
    public class WorkspaceGenerator : CopyGenerator
    {
        public WorkspaceGenerator(GeneratorContext DbContext, ILog logger, IConfiguration configuration) : base(DbContext, logger, configuration)
        {
        }

        public override string GeneratorName => "Workspace";

        public void Setup(Application app)
        {
            Application = app;
            MainDefinition = app.Definition;

            SOURCE_DIRECTORY = Configuration.GetValue<string>("WORKSPACES_TEMPLATES_DIR");
            TARGET_DIRECTORY = APPLICATIONS_DIRECTORY.CombineWith(app.Name);
        }

        public override List<Archive> Run(bool force = false)
        {
            Log.Info($"Running Workspace: [{Application?.Name}]");

            return base.Run(force);
        }
    }
}
