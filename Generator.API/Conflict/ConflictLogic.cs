using DiffPlex.DiffBuilder.Model;
using ServiceStack;
using ServiceStack.OrmLite;
using System.Data;

namespace Generator.API;
public class ConflictLogic : WriteLogic<Conflict>, ILogicWriteAsync<Conflict>
{
    public ConflictLogic(GeneratorContext DbContext, ILog logger, IConfiguration configuration) : base(DbContext, logger, configuration)
    {
    }

    public Conflict? Find(Conflict criteria)
    {
        var query = GetAll();

        query = query.Where(c => c.Generator == criteria.Generator)
            .Where(c => c.SubGenerator == criteria.SubGenerator)
            .Where(c => c.Application == null || c.Application == criteria.Application)
            .Where(c => c.GeneratorSource == criteria.GeneratorSource)
            //.Where(c => c.ApplicationSource == criteria.ApplicationSource)
            .Where(c => c.LineBefore == criteria.LineBefore)
            .Where(c => c.LineAfter == criteria.LineAfter)
            .ToList();

        return query.FirstOrDefault();
    }

    public Diff ResolveConflicts(Diff diff, Archive f, bool force = false)
    {
        if (force)
        {
            Log.Info($"Skip: Resolve Conflicts for: [{f.FileName}], (--force)");
            f.Content = string.Join("\n", diff.OldText!.Lines!.Select(l => l.Text));
            return diff;
        }

        Log.Info($"Resolve Conflicts for: [{f.FileName}]");

        bool generatedZone = false;
        bool slotZone = false;

        for (var i = diff.NewText!.Lines!.Count - 1; i >= 0; i--)
        {
            var line = diff.NewText.Lines[i];
            var lineBefore = diff.NewText.Lines.ElementAtOrDefault(i - 1);
            var lineAfter = diff.NewText.Lines.ElementAtOrDefault(i + 1);

            if (line.Text != null && line.Text.Contains("end:generated"))
                generatedZone = true;

            if (lineAfter?.Text != null && lineAfter.Text.Contains("start:generated"))
                generatedZone = false;

            if (lineAfter?.Text != null && lineAfter.Text.Contains("end:slot"))
                slotZone = true;

            if (slotZone)
            {
                if (line.Type != ChangeType.Unchanged)
                {
                    line.Ignored = true;
                    f.ComparisionResult.Add(ArchiveComparisionResult.Slot);
                    diff.NewText.Lines[i].Text = diff.OldText!.Lines![i].Text;
                }

                if (lineBefore?.Text != null && lineBefore.Text.Contains("start:slot"))
                    slotZone = false;

                continue;
            }

            if (line.Type != ChangeType.Unchanged)
            {
                if (generatedZone)
                {
                    f.ComparisionResult.Add(ArchiveComparisionResult.Generated);
                    //diff.NewText.Lines[i].Text = diff.OldText.Lines[i].Text;
                    continue;
                }

                var generatorLine = diff.OldText!.Lines![i];

                var knownConflict = Find(new Conflict
                {
                    Application = f.ProjectName,
                    Generator = f.Generator,
                    SubGenerator = f.SubGenerator,
                    ApplicationSource = line?.Text,
                    GeneratorSource = generatorLine?.Text,
                    LineBefore = lineBefore?.Text,
                    LineAfter = lineAfter?.Text
                });

                if (knownConflict != null)
                    switch (knownConflict.Resolution)
                    {
                        case Conflict.ConflictResolution.OverwriteApp:
                        case Conflict.ConflictResolution.OverwriteAll:
                            f.ComparisionResult.Add(ArchiveComparisionResult.Overwrite);

                            if (knownConflict.ChangeType == "Imaginary")
                            {
                                diff.NewText.Lines.RemoveAt(i);
                                continue;
                            }
                            else
                                diff.NewText.Lines[i].Text = diff.OldText.Lines[i].Text;

                            break;

                        case Conflict.ConflictResolution.IgnoreApp:
                        case Conflict.ConflictResolution.IgnoreAll:
                            line!.Ignored = true;
                            diff.OldText.Lines[i].Ignored = true;
                            f.ComparisionResult.Add(ArchiveComparisionResult.Ignore);
                            break;
                        default:
                            f.ComparisionResult.Add(ArchiveComparisionResult.Conflict);
                            break;
                    }
                else
                    f.ComparisionResult.Add(ArchiveComparisionResult.Conflict);
            }
        }

        f.Content = string.Join("\n", diff.NewText.Lines.Select(l => l.Text));

        return diff;
    }
}
