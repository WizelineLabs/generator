using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using ServiceStack;

namespace Generator.API.BaseGenerators;

public abstract class CopyGenerator : BaseGenerator, IGenerator
{
    public string SOURCE_DIRECTORY { get; set; } = "";
    public string TARGET_DIRECTORY { get; set; } = "";

    protected CopyGenerator(DbContext DbContext, ILog logger, IConfiguration configuration) : base(DbContext, logger, configuration)
    {
    }

    public override List<Archive> Run(bool force = false)
    {
        if (string.IsNullOrWhiteSpace(MainDefinition?.ProjectName))
            throw new Exception("Invalid [ProjectName] Variable");

        if (string.IsNullOrWhiteSpace(GeneratorName))
            throw new Exception("Invalid [GeneratorName] Variable");

        #region Read Files
        Log.Info("CopyGenerator.Run ReadFiles");
        var generatorFiles = GetFiles(SOURCE_DIRECTORY, SOURCE_DIRECTORY, GeneratorName, MainDefinition.ProjectName);
        var applicationFiles = GetFiles(TARGET_DIRECTORY, TARGET_DIRECTORY, GeneratorName, MainDefinition.ProjectName);
        #endregion

        #region Left Side - Generator Only Files
        Log.Info("CopyGenerator.Run Generator Only Files");
        var generatorOnlyFiles = generatorFiles.Where(a => !applicationFiles
            .Any(b => b.FileName == a.FileName && b.RelativePath == a.RelativePath))
            .ToList();

        foreach (var f in generatorOnlyFiles)
        {
            f.Generator = "generator-ssr";

            f.ComparisionResult.Add(ArchiveComparisionResult.GeneratorOnly);
            f.ComparisionResult = f.ComparisionResult.Distinct().ToList();
            f.LeftPath = f.DirectoryPath;
            f.RightPath = TARGET_DIRECTORY.CombineWith(f.RelativePath, f.FileName);
        }
        #endregion

        #region Right Side - Application Only Files
        Log.Info("CopyGenerator.Run Application Only Files");
        var appOnlyFiles = applicationFiles.Where(a => !generatorFiles
            .Any(b => b.FileName == a.FileName && b.RelativePath == a.RelativePath))
            .ToList();

        foreach (var f in appOnlyFiles)
        {
            f.Generator = "generator-ssr";

            f.ComparisionResult.Add(ArchiveComparisionResult.AppOnly);
            f.ComparisionResult = f.ComparisionResult.Distinct().ToList();
            f.RightPath = f.FullPath;
            f.LeftPath = SOURCE_DIRECTORY.CombineWith(f.RelativePath, f.FileName);
        }
        #endregion

        #region Both Sides - Need to Compare Content
        Log.Info("CopyGenerator.Run Both Sides Files");
        var bothIncluded = generatorFiles.Where(a => applicationFiles
            .Any(b => b.FileName == a.FileName && b.RelativePath == a.RelativePath))
            .ToList();

        foreach (var f in bothIncluded)
        {
            f.Generator = "generator-ssr";

            var diffModel = CompareFiles(f, force);
            // ConflictLogic.ResolveConflicts(diffModel, f, force);
            f.ComparisionResult = f.ComparisionResult.Distinct().ToList();
        }

        #endregion

        #region Write
        Log.Info("CopyGenerator.Run Write.GeneratorOnlyFiles");
        foreach (var file in generatorOnlyFiles)
        {
            file.ComparisionResult.Add(ArchiveComparisionResult.Added);
            CopyFile(file, TARGET_DIRECTORY.CombineWith(file.RelativePath, file.FileName));
        }

        Log.Info("CopyGenerator.Run Overwrite Archives:");
        foreach (var file in bothIncluded.Where(f => force == true || f.ComparisionResult.Contains(ArchiveComparisionResult.Overwrite)))
            File.WriteAllText(file.RightPath!, file.Content);
        #endregion

        var result = new List<Archive>();
        result.AddRange(generatorOnlyFiles);
        result.AddRange(appOnlyFiles);
        result.AddRange(bothIncluded);

        result.Sort((x, y) => x.RelativePath.CompareTo(y.RelativePath));

        return result;
    }

    public Diff CompareFiles(Archive f, bool force = false)
    {
        f.LeftPath = SOURCE_DIRECTORY.CombineWith(f.RelativePath, f.FileName);
        f.RightPath = TARGET_DIRECTORY.CombineWith(f.RelativePath, f.FileName);


        var aFile = new FileInfo(f.LeftPath);
        var generatorFileContent = aFile.ReadAllText();
        foreach (var variable in Variables)
            generatorFileContent = generatorFileContent.Replace(variable.Key, variable.Value);

        generatorFileContent = Format(generatorFileContent, aFile.Extension);


        FileInfo bFile; string? appFileContent;
        if (!force)
        {
            bFile = new FileInfo(f.RightPath);
            appFileContent = bFile.ReadAllText();

            appFileContent = Format(appFileContent, bFile.Extension);

            var model = new SideBySideDiffBuilder(new Differ()).BuildDiffModel(generatorFileContent, appFileContent, false)
                .ConvertTo<Diff>();

            if (model.OldText!.Lines!.All(l => l.Type == ChangeType.Unchanged))
                if (model.NewText!.Lines!.All(l => l.Type == ChangeType.Unchanged))
                {
                    f.ComparisionResult.Add(ArchiveComparisionResult.Identical);
                    Log.Info($"Compare File: [{f.FileName}], Identical.");
                    return model;
                }
            Log.Info($"Compare File: [{f.FileName}], Different.");
            return model;
        }
        else
        {
            Log.Info($"Compare File: [{f.FileName}], Skip (--Force).");
            f.ComparisionResult.Add(ArchiveComparisionResult.Identical);
            return new SideBySideDiffBuilder(new Differ()).BuildDiffModel(generatorFileContent, "", false)
            .ConvertTo<Diff>();
        }
    }
}
