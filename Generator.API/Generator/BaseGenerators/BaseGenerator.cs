using System.Diagnostics;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using ServiceStack;

namespace Generator.API.BaseGenerators;

public interface IGenerator
{
    string GeneratorName { get; }
    List<Archive> Run(bool force = false);
}

public abstract class BaseGenerator : BaseLogic, IGenerator
{
    public abstract string GeneratorName { get; }

    public string APPLICATIONS_DIRECTORY;
    public string FORMAT_PROGRAM;
    public string FORMAT_PROGRAM_ARGS;

    public Application? Application { get; set; }
    public MainDefinition? MainDefinition { get; set; }
    public Dictionary<string, string> Variables = new Dictionary<string, string>();
    public List<Archive> Ignored = new List<Archive>();
    public List<Archive> IncludeAfterIgnored = new List<Archive>();

    private AdhocWorkspace workspace = new AdhocWorkspace();

    // public ConflictLogic ConflictLogic { get; set; }

    protected BaseGenerator(DbContext DbContext, ILog logger, IConfiguration configuration) : base(DbContext, logger, configuration)
    {
        APPLICATIONS_DIRECTORY = configuration.GetValue<string>("APPLICATIONS_DIR");
        FORMAT_PROGRAM = configuration.GetValue<string>("BACKEND_FORMAT");
        FORMAT_PROGRAM_ARGS = configuration.GetValue<string>("BACKEND_FORMAT_ARGS");
    }

    abstract public List<Archive> Run(bool force = false);

    public void CopyFile(Archive file, string toPath)
    {
        if (file.FileName == null) return;

        var f = new FileInfo(file.DirectoryPath.CombineWith(file.FileName));
        if (!Directory.Exists(toPath)) Directory.CreateDirectory(toPath.Substring(0, toPath.Length - file.FileName.Length));

        if (!new string[] { ".cs", ".tsx", ".nowignore", ".json", ".js", ".dockerignore", ".gitignore", ".yml", ".ts", ".scss", ".sass", ".html" }.Contains(f.Extension))
        {
            f.CopyTo(toPath);
            return;
        }

        string? tempLineValue;
        using (FileStream inputStream = f.OpenRead())
        using (StreamReader inputReader = new StreamReader(inputStream))
        using (StreamWriter outputWriter = File.AppendText(toPath))
            while (null != (tempLineValue = inputReader.ReadLine()))
            {
                foreach (var variable in Variables)
                    tempLineValue = tempLineValue.Replace(variable.Key, variable.Value);

                outputWriter.WriteLine(tempLineValue);
            }

        Log.Info($"Copied Template file: [{f.Name}] to Path: [{toPath}]");
    }

    public string InterpolateVariables(string content, Dictionary<string, object>? moreVariables = null)
    {
        var context = new ScriptContext
        {
            ScriptBlocks = { new WithScriptBlock() },
            ScriptMethods = { new GeneratorMethods() }
        }.Init();
        Variables.ForEach((key, value) => context.Args.Add(key, value));
        moreVariables?.ForEach((key, value) => context.Args.Add(key, value));

        return context.RenderScript(content);
    }

    class GeneratorMethods : ScriptMethods
    {
        public string? Slot(ScriptScopeContext scope, string name)
        {
            if (name == null)
                return null;

            var slotValue = scope.Context.Args.ContainsKey(name) ? scope.Context.Args[name] as string : null;
            if (slotValue == null)
            {
                slotValue = scope.ScopedParams.ContainsKey(name) ? scope.ScopedParams[name] as string : null;
                if (slotValue == null)
                    return null;
            }

            var split = slotValue.Split(new string[] { "\\n" }, StringSplitOptions.None);
            for (int i = 0; i < split.Length; i++)
                split[i] = split[i].Replace("\\", "");

            var result = split.Join("\n");

            return $@"///start:slot:{name.ToLower()}<<<{"\n"}{result}{"\n"}///end:slot:{name.ToLower()}<<<";
        }

        public string? Generated(ScriptScopeContext scope, string name)
        {
            if (name == null) return null;

            var generated = scope.Context.Args.ContainsKey("generated") ? scope.Context.Args["generated"] as Dictionary<string, object> : null;
            if (generated == null) return null;

            if (!generated.ContainsKey(name)) return null;

            var result = generated[name] as string;
            if (result == null) return null;

            if (name == "Content")
                return $@"{{/* ///start:generated:{name.ToLower()}<<< */}}{"\n"}{result}{"\n"}{{/* ///end:generated:{name.ToLower()}<<< */}}";

            return $@"///start:generated:{name.ToLower()}<<<{"\n"}{result}{"\n"}///end:generated:{name.ToLower()}<<<";
        }
    }

    public string? WriteFile(string? content, string? toPath, string? fileName, bool format = false)
    {
        if (content == null || toPath == null || fileName == null) return null;

        if (!Directory.Exists(toPath))
            Directory.CreateDirectory(toPath.Substring(0, toPath.Length - fileName.Length));

        //InterpolateVariables(result);
        if (format)
            content = Format(content, new FileInfo(toPath).Extension);

        File.WriteAllText(toPath, content);

        Log.Info($"Written file: [{fileName}] to Path: [{toPath}]");

        return content;
    }

    virtual public List<Archive> GetFiles(string path, string basePath, string fileType, string projectName, string? frontendName = "", bool? ignoreCache = false) //ignoreCache getting subdiretories.
    {
        //var cacheKey = $"GetFiles_{fileType}_{projectName}_{frontendName}_{path}";
        //var cache = Cache.Get<List<Archive>>(cacheKey);
        //if (cache != null && !ignoreCache)
        //    return cache;

        var files = new List<Archive>();

        var relativePath = GetRelativePath(path, basePath);
        var toIgnore = Ignored.Any(f => relativePath.StartsWith(f.RelativePath)
            && !IncludeAfterIgnored.Any(i => relativePath.StartsWith(i.RelativePath)));

        if (toIgnore)
        {
            Log.Info($"IGNORE: [{relativePath}]");
            return files;
        }


        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
        {
            var directory = new DirectoryInfo(path);
            foreach (var file in directory.EnumerateFiles())
            {
                var f = new Archive
                {
                    ProjectName = projectName,
                    FrontendName = frontendName!,
                    SubGenerator = GeneratorName,
                    FileName = file.Name,
                    DirectoryPath = file.DirectoryName!,
                    FullPath = file.FullName,
                    FileType = fileType,
                    RelativePath = GetRelativePath(file.DirectoryName!, basePath)
                };
                files.Add(f);
            }

            foreach (var subfolder in directory.EnumerateDirectories())
                files.AddRange(GetFiles(subfolder.FullName, basePath, fileType, projectName, frontendName, true));
        }

        //if (!ignoreCache) Cache.Set(cacheKey, files);
        return files;
    }

    public string GetRelativePath(string fullPath, string basePath, string? fileName = null)
    {
        try
        {
            var result = fullPath.Substring(basePath.Length);

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                if (result.EndsWith(fileName))
                    result = result.Substring(0, result.Length - fileName.Length);
            }

            if (result == "") result = "/";

            return result;
        }
        catch (Exception ex)
        {
            Log.Error($"Get Relative Path: fullPath: [{fullPath}] basePath: [{basePath}] fileName: [{fileName}]", ex);
            return "";
        }
    }

    public Diff CompareContent(string? leftContent, string? rightContent, Archive f, string extension, bool force = false)
    {
        foreach (var variable in Variables)
            leftContent = leftContent?.Replace(variable.Key, variable.Value);

        leftContent = Format(leftContent, extension);

        if (force)
        {
            var forceModel = new SideBySideDiffBuilder(new Differ()).BuildDiffModel(leftContent, "", false)
                .ConvertTo<Diff>();
            f.ComparisionResult.Add(ArchiveComparisionResult.Identical);

            return forceModel;
        }

        rightContent = Format(rightContent, extension);
        var model = new SideBySideDiffBuilder(new Differ()).BuildDiffModel(leftContent, rightContent, false)
            .ConvertTo<Diff>();

        if (model.OldText.Lines.All(l => l.Type == ChangeType.Unchanged))
            if (model.NewText.Lines.All(l => l.Type == ChangeType.Unchanged))
            {
                f.ComparisionResult.Add(ArchiveComparisionResult.Identical);
                return model;
            }

        return model;
    }

    public static string ToVariable(string from)
    {
        return from.ToTitleCase().Split(' ').Map(s => s.Trim()).Join("");
        //return from.ToPascalCase();
    }

    //Action<string,string,string> = <Property, Alias, Array>
    public void FromAlias(string from, Action<string, string, string[]> action)
    {
        if (string.IsNullOrWhiteSpace(from))
            action("", "", new string[] { });
        else
        {
            var arr = from.Split(new string[] { " as " }, StringSplitOptions.RemoveEmptyEntries);
            action(ToVariable(arr[0]), arr.ElementAtOrDefault(1) ?? arr[0], arr);
        }
    }

    public void FromPipeOptions(string from, Action<List<string>> options)
    {
        var pipeOptions = from.Split('|').Map(o => o.Trim());
        options(pipeOptions);
    }

    public static string? InsertText(string? current, int startPosition, int endPosition, string? text)
    {
        return current?.Substring(0, startPosition) + text + current?.Substring(endPosition);
    }

    public void InsertGeneratedText(string filePath, string slotName, Action<List<string>> populateToInsert, string newLine = "\n            ")
    {
        var toInsert = new List<string>();

        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists) return;
        var fileContent = fileInfo.ReadAllText();

        populateToInsert(toInsert);

        var toAdd = toInsert.Count > 0 ? $"{newLine}{toInsert.Join(newLine)}{newLine}" : null;

        fileContent = Format(OverwriteSlot(fileContent, slotName, toAdd), fileInfo.Extension);

        File.WriteAllText(fileInfo.FullName, fileContent);
    }

    public static string? OverwriteSlot(string? fileContent, string? slotName, string? content = "\n")
    {
        var startSlot = $"///start:{slotName}<<<";
        var endSlot = $"///end:{slotName}<<<";

        int startPostion = -1;
        int endPosition = -1;

        if (!string.IsNullOrWhiteSpace(fileContent))
        {
            startPostion = fileContent.IndexOf(startSlot);
            endPosition = fileContent.IndexOf(endSlot);
        }

        return InsertText(fileContent, startPostion + startSlot.Length, endPosition, content);
    }

    public string? Format(string? content = "", string? extension = "")
    {
        // Standardize spacing:
        if (content != null)
        {
            var split = content.Split('\n').ToList();
            var spacesCount = 0;
            for (int i = split.Count - 1; i >= 0; i--)
            {
                var current = split[i];
                if (string.IsNullOrWhiteSpace(current))
                {
                    spacesCount++;
                    if (spacesCount > 1)
                    {
                        split.RemoveAt(i);
                        spacesCount--;
                    }
                }
                else
                    spacesCount = 0;
            }
            content = split.Join("\n");
        }

        if (!new string[] { ".cs", ".tsx", ".js" }.Contains(extension))
            return content;

        if (".cs" == extension)
        {
            var root = CSharpSyntaxTree.ParseText(content!, encoding: Encoding.UTF8).GetRoot();
            var result = Formatter.Format(root, workspace);
            var resulting = result.GetText().ToString();
            return resulting;
        }

        var formatProgramArgs = FORMAT_PROGRAM_ARGS.Replace("file", $"sample{extension}");

        var parser = "";
        if (new string[] { ".tsx", ".ts" }.Contains(extension))
            parser = "typescript";

        if (new string[] { ".js", ".jsx" }.Contains(extension))
            parser = "babel";

        if (new string[] { ".css", ".scss", "less" }.Contains(extension))
            parser = "css";

        if (new string[] { ".json", ".json5" }.Contains(extension))
            parser = "json";

        if (new string[] { ".md" }.Contains(extension))
            parser = "markdown";

        if (new string[] { ".html" }.Contains(extension))
            parser = "html";

        if (new string[] { ".yaml", ".yml" }.Contains(extension))
            parser = "yaml";

        if (parser != "")
            formatProgramArgs = FORMAT_PROGRAM_ARGS.Replace("parserSlot", parser);

        Log.Info($"Format file extension: [{extension}] FORMAT_PROGRAM: [{FORMAT_PROGRAM}] ARGS: [{formatProgramArgs}]");

        var startInfo = new ProcessStartInfo(FORMAT_PROGRAM, formatProgramArgs)
        {
            UseShellExecute = false,
            RedirectStandardError = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
        };

        using (var process = new Process())
        {
            process.StartInfo = startInfo;
            process.ErrorDataReceived += CaptureError;

            process.Start();

            process.StandardInput.Write(content);
            process.StandardInput.Close();
            //process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();

            if (string.IsNullOrWhiteSpace(output))
                return content;

            //var error = process.StandardError.ReadToEnd();
            //if (!string.IsNullOrWhiteSpace(error))
            //    return content;

            return output;
        }
    }

    static void CaptureError(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
            throw new Exception(e.Data);
    }
}

public class Archive : ArchiveDTO
{
    public Archive()
    {

    }
    public Archive(string projectName, string fileType, string frontendName, string relativePath, string fileName, ArchiveComparisionResult ComparisionResult)
    {
        ProjectName = projectName;
        FileType = fileType;
        FrontendName = frontendName;
        RelativePath = relativePath;
        FileName = fileName;
        ComparisionResult.Add(ComparisionResult);
    }

    public string? DirectoryPath { get; set; }
    public string? FullPath { get; set; }
    public new List<ArchiveComparisionResult> ComparisionResult = new List<ArchiveComparisionResult>();
    public string? Content { get; set; }
    public Diff? Diff { get; set; }
}

public class ArchiveDTO
{
    public string? FileName { get; set; }
    public string RelativePath { get; set; } = "";
    public string? FileType { get; set; }
    public List<string> ComparisionResult { get; set; } = new List<string>();
    public string? LeftPath { get; set; }
    public string? RightPath { get; set; }
    public string? Generator { get; set; }
    public string? SubGenerator { get; set; }
    public int DiffIndex { get; set; }
    public string? ProjectName { get; internal set; }
    public string? FrontendName { get; set; }
    public string? ComponentName { get; set; }
}

public enum ArchiveComparisionResult
{
    GeneratorOnly,
    AppOnly,
    Identical,
    Ignore,
    Overwrite,
    Conflict,
    Added,
    Generated,
    Slot
}
