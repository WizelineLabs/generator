
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using ServiceStack;

namespace Generator.API;

public class DiffLogic : WriteLogic<Diff>, ILogicWriteAsync<Diff>
{
    public DiffLogic(GeneratorContext DbContext, ILog logger, IConfiguration configuration, ConflictLogic conflictLogic
        , ApplicationLogic applicationLogic, EntityGenerator entityGenerator, FrontendGenerator frontendGenerator
        , ComponentGenerator componentGenerator) : base(DbContext, logger, configuration)
    {
        ConflictLogic = conflictLogic;
        ApplicationLogic = applicationLogic;
        EntityGenerator = entityGenerator;
        FrontendGenerator = frontendGenerator;
        ComponentGenerator = componentGenerator;
    }

    public ConflictLogic? ConflictLogic { get; set; }
    public ApplicationLogic? ApplicationLogic { get; set; }
    public EntityGenerator? EntityGenerator { get; set; }
    public FrontendGenerator? FrontendGenerator { get; set; }
    public ComponentGenerator? ComponentGenerator { get; set; }

    public Diff? Diff(Archive f)
    {
        if (f.FileType == "Entity" || f.FileType == "Logic" || f.FileType == "Gateway")
            return DiffEntity(f);

        if (f.FileType == "Frontend")
            return DiffFrontend(f);

        if (!string.IsNullOrWhiteSpace(f.ComponentName))
            return DiffComponent(f);

        Log.Info($"Diff file: [{f.FileName}] of type: [{f.FileType}]");

        string leftFileContent = "";
        string rightFileContent = "";

        var leftFile = new FileInfo(f.LeftPath!);
        var rightFile = new FileInfo(f.RightPath!);

        if (leftFile.Exists)
        {
            leftFileContent = leftFile.ReadAllText();
        }

        if (rightFile.Exists)
        {
            rightFileContent = rightFile.ReadAllText();
            //ToDo: uncomment line when EntityGenerator is ready
            //rightFileContent = EntityGenerator.Format(rightFileContent, rightFile.Extension);
        }

        var model = new SideBySideDiffBuilder(new Differ()).BuildDiffModel(leftFileContent, rightFileContent, false)
            .ConvertTo<Diff>();

        return ConflictLogic!.ResolveConflicts(model, f);
    }

    public Diff? DiffEntity(Archive f)
    {
        Log.Info($"Diff Entity file: [{f.FileName}]");

        var app = ApplicationLogic!.GetAll().FirstOrDefault(a => a.Name == f.ProjectName);
        if (app == null)
            throw new KnownError($"Application [{f.ProjectName}] no longer exists");

        ApplicationLogic.AdapterOut(app);

        var entity = app.Definition!.Entities.FirstOrDefault(e => f.FileName!.StartsWith(e.Name));
        if (entity == null)
            throw new KnownError($"Entity [{f.FileName}] no longer exists.");

        //ToDo: uncomment next lines when EntityGenerator is ready
        //EntityGenerator.SetApplication(app);
        //EntityGenerator.Setup(entity);

        // if (f.FileType == "Entity")
        //     return EntityGenerator.GenerateEntity(entity).Diff;

        // if (f.FileType == "Logic")
        //     return EntityGenerator.GenerateLogic(entity).Diff;

        // if (f.FileType == "Gateway")
        //     return EntityGenerator.GenerateEndpoint(entity).Diff;

        return null;
    }

    public Diff DiffComponent(Archive f)
    {
        Log.Info($"Diff Component file: [{f.FileName}]");

        var app = ApplicationLogic!.GetAll().FirstOrDefault(a => a.Name == f.ProjectName);
        if (app == null)
            throw new KnownError($"Application [{f.ProjectName}] does not exist.");

        ApplicationLogic.AdapterOut(app);

        var frontend = app.Definition!.Frontends.FirstOrDefault(a => a.Name == f.FrontendName);
        if (frontend == null)
            throw new KnownError($"Frontend [{f.FrontendName}] does not exist.");

        var component = app.Definition!.Components.FirstOrDefault(e => f.FileName!.StartsWith(e.Name));
        if (component == null)
            throw new KnownError($"Entity [{f.FileName}] does not exist.");

        //ToDo: uncomment next lines when ComponentGenerator is ready
        //ComponentGenerator.Setup(app, component);

        //return ComponentGenerator.GenerateComponent(component, frontend.Name).Diff;
        return null;
    }

    public Diff DiffFrontend(Archive f)
    {
        Log.Info($"Diff Frontend file: [{f.FileName}]");

        var app = ApplicationLogic!.GetAll().FirstOrDefault(a => a.Name == f.ProjectName);
        if (app == null)
            throw new KnownError($"Application [{f.ProjectName}] no longer exists");

        ApplicationLogic.AdapterOut(app);

        var frontend = app.Definition!.Frontends.FirstOrDefault(fr => fr.Name == f.FrontendName);
        if (frontend == null)
            throw new KnownError($"Frontend [{f.FrontendName}] no longer exists.");

        FrontendGenerator!.Setup(app, frontend);

        string leftFileContent = "";
        string rightFileContent = "";

        var leftFile = new FileInfo(f.LeftPath!);
        var rightFile = new FileInfo(f.RightPath!);

        if (leftFile.Exists)
        {
            leftFileContent = leftFile.ReadAllText();
        }

        if (rightFile.Exists)
        {
            rightFileContent = rightFile.ReadAllText();
            rightFileContent = FrontendGenerator.Format(rightFileContent, rightFile.Extension)!;
        }

        var model = new SideBySideDiffBuilder(new Differ()).BuildDiffModel(leftFileContent, rightFileContent, false)
            .ConvertTo<Diff>();

        return ConflictLogic!.ResolveConflicts(model, f);
    }

    public Diff? CopyToRight(Archive file, DiffLine selectedLine)
    {
        Log.Info($"Copy to Right: [{file.FileName}], selectedLine: [{selectedLine.Text}]");

        var diffModel = Diff(file);
        var leftLine = diffModel!.OldText!.Lines![selectedLine.Index];

        if (leftLine.Text != selectedLine.Text)
            throw new KnownError($"State is not the same, please reload.");

        if (leftLine.Type == ChangeType.Imaginary)
            diffModel.NewText!.Lines!.RemoveAt(selectedLine.Index);
        else
            diffModel.NewText!.Lines![selectedLine.Index].Text = leftLine.Text;

        File.WriteAllText(file.RightPath!, string.Join("\n", diffModel.NewText.Lines.Select(l => l.Text)));

        return Diff(file);
    }

    public Diff? FeedbackGenerator(Archive file, DiffLine selectedLine)
    {
        Log.Info($"Feedback Generator: [{file.FileName}], selectedLine: [{selectedLine.Text}]");

        var diffModel = Diff(file);
        var rightLine = diffModel!.NewText!.Lines![selectedLine.Index];

        if (rightLine.Text != selectedLine.Text)
            throw new KnownError($"State is not the same, please reload.");

        var lineBefore = diffModel.OldText!.Lines!.ElementAtOrDefault(selectedLine.Index - 1);
        var lineAfter = diffModel.OldText!.Lines!.ElementAtOrDefault(selectedLine.Index + 1);

        Conflict conflict = new Conflict
        {
            Application = null, //Applies to all applications.
            ChangeType = rightLine.Type.ToString(),
            DiffModel = diffModel.ToJson(),
            FileName = file.FileName,
            Generator = file.Generator,
            SubGenerator = file.SubGenerator,
            Position = rightLine.Position,
            DiffIndex = selectedLine.Index,
            RelativePath = file.RelativePath,
            Resolution = Conflict.ConflictResolution.OverwriteAll,
            GeneratorSource = rightLine.Text,
            ApplicationSource = diffModel.OldText!.Lines![selectedLine.Index].Text,
            LineBefore = lineBefore?.Text,
            LineAfter = lineAfter?.Text
        };

        var conflictAlreadyExists = ConflictLogic!.Find(conflict);
        if (conflictAlreadyExists == null)
        {
            ConflictLogic.Add(conflict);
            Log.Info($"Conflict entry added.");
        }

        if (rightLine.Type == ChangeType.Imaginary)
            diffModel.OldText.Lines.RemoveAt(selectedLine.Index);
        else
            diffModel.OldText.Lines[selectedLine.Index].Text = rightLine.Text;

        File.WriteAllText(file.LeftPath!, string.Join("\n", diffModel.OldText.Lines.Select(l => l.Text)));

        return Diff(file);
    }

    public object? CopyToAllApps(Archive file, DiffLine selectedLine)
    {
        Log.Info($"Copy To All Apps: [{file.FileName}], selectedLine: [{selectedLine.Text}]");

        var diffModel = Diff(file);
        var leftLine = diffModel!.OldText!.Lines![selectedLine.Index];

        if (leftLine.Text != selectedLine.Text)
            throw new KnownError($"State is not the same, please reload.");

        var lineBefore = diffModel.NewText!.Lines!.ElementAtOrDefault(selectedLine.Index - 1);
        var lineAfter = diffModel.NewText!.Lines!.ElementAtOrDefault(selectedLine.Index + 1);

        Conflict conflict = new Conflict
        {
            Application = null, //Applies to all applications.
            ChangeType = leftLine.Type.ToString(),
            DiffModel = diffModel.ToJson(),
            FileName = file.FileName,
            Generator = file.Generator,
            SubGenerator = file.SubGenerator,
            Position = leftLine.Position,
            DiffIndex = selectedLine.Index,
            RelativePath = file.RelativePath,
            Resolution = Conflict.ConflictResolution.OverwriteAll,
            GeneratorSource = leftLine.Text,
            ApplicationSource = diffModel.NewText.Lines![selectedLine.Index].Text,
            LineBefore = lineBefore?.Text,
            LineAfter = lineAfter?.Text
        };

        var conflictAlreadyExists = ConflictLogic!.Find(conflict);
        if (conflictAlreadyExists == null)
        {
            ConflictLogic.Add(conflict);
            Log.Info($"Conflict entry added.");
        }

        if (leftLine.Type == ChangeType.Imaginary)
            diffModel.NewText.Lines.RemoveAt(selectedLine.Index);
        else
            diffModel.NewText.Lines[selectedLine.Index].Text = leftLine.Text;

        File.WriteAllText(file.RightPath!, string.Join("\n", diffModel.NewText.Lines.Select(l => l.Text)));

        return Diff(file);
    }

    public Diff? IgnoreForAllApps(Archive file, DiffLine selectedLine)
    {
        Log.Info($"Ignore For All Apps: [{file.FileName}], selectedLine: [{selectedLine.Text}]");

        var diffModel = Diff(file);
        var leftLine = diffModel!.OldText!.Lines![selectedLine.Index];

        if (leftLine.Text != selectedLine.Text)
            throw new KnownError($"State is not the same, please reload.");

        var lineBefore = diffModel.NewText!.Lines!.ElementAtOrDefault(selectedLine.Index - 1);
        var lineAfter = diffModel.NewText!.Lines!.ElementAtOrDefault(selectedLine.Index + 1);

        Conflict conflict = new Conflict
        {
            Application = null, //Applies to all applications.
            ChangeType = leftLine.Type.ToString(),
            DiffModel = diffModel.ToJson(),
            FileName = file.FileName,
            Generator = file.Generator,
            SubGenerator = file.SubGenerator,
            Position = leftLine.Position,
            DiffIndex = selectedLine.Index,
            RelativePath = file.RelativePath,
            Resolution = Conflict.ConflictResolution.IgnoreAll,
            GeneratorSource = leftLine.Text,
            ApplicationSource = diffModel.NewText!.Lines![selectedLine.Index].Text,
            LineBefore = lineBefore?.Text,
            LineAfter = lineAfter?.Text
        };

        var conflictAlreadyExists = ConflictLogic!.Find(conflict);
        if (conflictAlreadyExists == null)
        {
            ConflictLogic.Add(conflict);
            Log.Info($"Conflict entry added.");
        }

        return Diff(file);
    }

    public Diff? IgnoreForApp(Archive file, DiffLine selectedLine)
    {
        Log.Info($"Ignore For App: [{file.FileName}], selectedLine: [{selectedLine.Text}]");

        var diffModel = Diff(file);
        var leftLine = diffModel!.OldText!.Lines![selectedLine.Index];

        if (leftLine.Text != selectedLine.Text)
            throw new KnownError($"State is not the same, please reload.");

        var lineBefore = diffModel.NewText!.Lines!.ElementAtOrDefault(selectedLine.Index - 1);
        var lineAfter = diffModel.NewText!.Lines!.ElementAtOrDefault(selectedLine.Index + 1);

        Conflict conflict = new Conflict
        {
            Application = file.ProjectName,
            ChangeType = leftLine.Type.ToString(),
            DiffModel = diffModel.ToJson(),
            FileName = file.FileName,
            Generator = file.Generator,
            SubGenerator = file.SubGenerator,
            Position = leftLine.Position,
            DiffIndex = selectedLine.Index,
            RelativePath = file.RelativePath,
            Resolution = Conflict.ConflictResolution.IgnoreAll,
            GeneratorSource = leftLine.Text,
            ApplicationSource = diffModel.NewText!.Lines![selectedLine.Index].Text,
            LineBefore = lineBefore?.Text,
            LineAfter = lineAfter?.Text
        };

        var conflictAlreadyExists = ConflictLogic!.Find(conflict);
        if (conflictAlreadyExists == null)
        {
            ConflictLogic.Add(conflict);
            Log.Info($"Conflict entry added.");
        }

        return Diff(file);
    }

    public void CopyFile(string fromPath, string toPath)
    {
        if (new FileInfo(toPath).Exists)
            throw new KnownError("File already exists.");

        var f = new FileInfo(fromPath);

        using (FileStream SourceStream = f.OpenRead())
        using (FileStream DestinationStream = File.Create(toPath))
            SourceStream.CopyTo(DestinationStream);

        Log.Info($"File copied from [{fromPath}] to [{toPath}]");
    }
}
