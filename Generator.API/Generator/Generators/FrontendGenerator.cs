using System.Diagnostics;
using Generator.API.BaseGenerators;
using ServiceStack;

namespace Generator.API.Generators;

public class FrontendGenerator : CopyGenerator
{
    public override string GeneratorName => "Frontend";
    public FrontendDefinition? Frontend { get; set; }

    public override string SOURCE_DIRECTORY { get; set; } = "";
    public override string TARGET_DIRECTORY { get; set; } = "";

    public FrontendGenerator(GeneratorContext DbContext, ILog logger, IConfiguration configuration) : base(DbContext, logger, configuration)
    {
        FORMAT_PROGRAM = configuration.GetValue<string>("FRONTEND_FORMAT");
        FORMAT_PROGRAM_ARGS = configuration.GetValue<string>("FRONTEND_FORMAT_ARGS");
    }

    public FrontendDefinition Parse(FrontendDefinition fromYaml)
    {
        if (fromYaml.DisplayName == null)
            fromYaml.DisplayName = fromYaml.Name;
        return fromYaml;
    }

    public void Setup(Application app, FrontendDefinition frontend)
    {
        Application = app;
        MainDefinition = app.Definition;
        Frontend = frontend;

        SOURCE_DIRECTORY = Configuration.GetValue<string>("FRONTEND_TEMPLATES_DIR");
        TARGET_DIRECTORY = APPLICATIONS_DIRECTORY.CombineWith(app.Name, "frontend", frontend.Name);

        Ignored = new List<Archive>
            {
                new Archive { RelativePath = @"\.next" },
                new Archive { RelativePath = @"\node_modules" },
                new Archive { RelativePath = @"yarn.lock" }
            };

        Variables = new Dictionary<string, string>
            {
                {"<%= frontendDisplayName %>", frontend.DisplayName! },
                {"<%= frontendName %>", frontend.Name! },
                {"<%= projectName %>", MainDefinition?.ProjectName! }
            };
    }

    public override List<Archive> Run(bool force = false)
    {
        Log.Info($"Running Frontend: [{Frontend?.Name!}]");

        var result = base.Run(force).Map(f =>
        {
            f.SubGenerator = GeneratorName;
            f.FrontendName = Frontend?.Name!;
            return f;
        });

        //Task.Run(() => InstallYarnDependencies(TargetDirectory));

        return result;
    }

    public void InstallYarnDependencies(string rootPath)
    {
        Log.Info($"Install Yarn Dependencies for Root Path: [{rootPath}]");

        var startInfo = new ProcessStartInfo("cmd.exe", $"/c cd {rootPath} && yarn")
        {
            UseShellExecute = true,
            RedirectStandardError = false,
            RedirectStandardOutput = false,
            CreateNoWindow = false
        };

        using (var process = new Process())
        {
            process.StartInfo = startInfo;
            process.Start();
            process.CloseMainWindow();
        }
    }

    public override List<Archive> GetFiles(string path, string basePath, string fileType, string projectName, string? frontendName = "", bool? ignoreCache = false)
    {
        return base.GetFiles(path, basePath, fileType, projectName, Frontend?.Name, ignoreCache);
    }
}
