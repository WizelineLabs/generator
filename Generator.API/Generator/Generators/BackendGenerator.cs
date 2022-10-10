using Generator.API.BaseGenerators;
using Reusable.CRUD.JsonEntities;

namespace Generator.API.Generators
{
    public class BackendGenerator : CopyGenerator
    {
        public BackendGenerator(GeneratorContext DbContext, ILog logger, IConfiguration configuration) : base(DbContext, logger, configuration)
        {
        }
        public override string GeneratorName => "Backend";

        public void Setup(Application app)
        {
            Application = app;
            MainDefinition = app.Definition;

            SOURCE_DIRECTORY = Configuration.GetValue<string>("BACKEND_TEMPLATES_DIR");
            TARGET_DIRECTORY = APPLICATIONS_DIRECTORY.CombineWith(app.Name, "backend");

            Ignored = new List<Archive>
            {
                new Archive{ RelativePath = @"\MyApp\bin" },
                new Archive{ RelativePath = @"\MyApp\obj" },
                new Archive{ RelativePath = @"\.vs" },
                new Archive{ RelativePath = @"\MyApp.API\bin" },
                new Archive{ RelativePath = @"\MyApp.API\obj" },
                new Archive{ RelativePath = @"\MyApp.API\" },
                new Archive{ RelativePath = @"\Reusable\obj" }
            };

            IncludeAfterIgnored = new List<Archive>
            {
                new Archive { RelativePath = @"\MyApp.API\Catalog"},
                new Archive { RelativePath = @"\MyApp.API\Account"}
            };

            Variables = new Dictionary<string, string>
            {
                {"<%= projectNameVariable %>", MainDefinition!.ProjectName },
                {"<%= projectName %>", MainDefinition!.ProjectName }
            };
        }

        public override List<Archive> Run(bool force = false)
        {
            Log.Info($"Running Backend: [{Application?.Name}]");

            var result = base.Run(force);

            var sln = result.FirstOrDefault(f => f.FileName == "MyApp.sln");
            if (sln != null)
            {
                var MyAppSlnPath = TARGET_DIRECTORY.CombineWith("MyApp.sln");
                sln.FileName = ToVariable(MainDefinition!.ProjectName);
                sln.RightPath = TARGET_DIRECTORY.CombineWith($"{ToVariable(MainDefinition.ProjectName)}.sln");

                if (!File.Exists(sln.RightPath))
                    File.Move(MyAppSlnPath, sln.RightPath);

                if (File.Exists(MyAppSlnPath))
                    File.Delete(MyAppSlnPath);
            }

            return result;
        }
    }
}
