using Spectre.Console;
using Spectre.Console.Rendering;

namespace MergeIncludes.Renderables;

/// <summary>
/// Renders the side-by-side folder structure and reference trees view
/// </summary>
public sealed class StructureAndReferenceView : IRenderable
{
    private readonly Table _table;

    /// <summary>
    /// Initializes a new instance of the StructureAndReferenceView class
    /// </summary>
    /// <param name="rootFile">The root file being processed</param>
    /// <param name="fileRelationships">Dictionary mapping files to their dependencies</param>
    public StructureAndReferenceView(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        if (rootFile == null) throw new ArgumentNullException(nameof(rootFile));
        if (fileRelationships == null) throw new ArgumentNullException(nameof(fileRelationships));

        _table = CreateTable(rootFile, fileRelationships);
    }

    /// <inheritdoc/>
    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        // Delegate to the internal table for measurement
        return ((IRenderable)_table).Measure(options, maxWidth);
    }

    /// <inheritdoc/>
    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        // Delegate to the internal table for rendering
        return ((IRenderable)_table).Render(options, maxWidth);
    }

    private static Table CreateTable(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        // Use the existing DefaultTreeBuilder to get the side-by-side tree lines
        var treeBuilder = new Services.DefaultTreeBuilder();
        var (folderLines, referenceLines) = treeBuilder.BuildSideBySideTrees(rootFile, fileRelationships);

        // Create a table with two columns for side-by-side display
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn(new TableColumn(""))
            .AddColumn(new TableColumn(""));

        // Add rows for each line, padding shorter lists with empty strings
        var maxRows = Math.Max(folderLines.Count, referenceLines.Count);
        for (int i = 0; i < maxRows; i++)
        {
            var folderLine = i < folderLines.Count ? folderLines[i] : "";
            var referenceLine = i < referenceLines.Count ? referenceLines[i] : "";
            table.AddRow(folderLine, referenceLine);
        }

        return table;
    }
}
