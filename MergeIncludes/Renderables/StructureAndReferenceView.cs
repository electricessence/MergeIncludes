using Spectre.Console;
using Spectre.Console.Rendering;

namespace MergeIncludes.Renderables;

/// <summary>
/// Renders the side-by-side folder structure and reference trees view using Tree widgets
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

        _table = CreateTwoColumnTreeTable(rootFile, fileRelationships);
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
    private static Table CreateTwoColumnTreeTable(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        // Create the left column: folder structure tree
        var folderTree = CreateFolderStructureTree(rootFile);

        // Create the right column: reference tree (similar to CreateBasicReferenceTree)
        var referenceTree = CreateReferenceTree(rootFile, fileRelationships);        // Create a table with two columns for side-by-side display, using minimum width
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .Collapse()  // Makes table take up minimal width
            .AddColumn(new TableColumn("Folder"))  // Auto-size to content
            .AddColumn(new TableColumn("References"));  // Auto-size to content

        // Add a single row with both trees
        table.AddRow(folderTree, referenceTree);

        return table;
    }

    private static Tree CreateFolderStructureTree(FileInfo rootFile)
    {
        // Create a simple folder tree showing the file location
        return TreeBuilders.FolderTreeBuilder.Create(rootFile.Directory!, new[] { rootFile });
    }
    private static Tree CreateReferenceTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        // Create root node with yellow color (not bold)
        var rootText = HyperLink.For(rootFile, new Style(Color.Yellow));
        var tree = new Tree(rootText);
        var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        BuildReferenceTreeRecursive(tree, rootFile.FullName, fileRelationships, processedFiles);

        return tree;
    }

    /// <summary>
    /// Recursively builds the reference tree hierarchy
    /// </summary>
    private static void BuildReferenceTreeRecursive(
        Tree tree, string filePath,
        Dictionary<string, List<string>> fileRelationships,
        HashSet<string> processedFiles)
    {
        if (processedFiles.Contains(filePath) || !fileRelationships.TryGetValue(filePath, out var dependencies))
            return;

        processedFiles.Add(filePath); foreach (var dependency in dependencies)
        {
            if (File.Exists(dependency) && !processedFiles.Contains(dependency))
            {
                var dependencyName = Path.GetFileName(dependency);
                // Add cyan color for dependency nodes
                var dependencyText = HyperLink.For(dependency, dependencyName, new Style(Color.Cyan1));
                var childNode = tree.AddNode(dependencyText);

                // Recursively build dependencies of this dependency
                BuildReferenceTreeNodeRecursive(childNode, dependency, fileRelationships, processedFiles);
            }
        }
    }

    /// <summary>
    /// Recursively builds child nodes in the reference tree
    /// </summary>
    private static void BuildReferenceTreeNodeRecursive(
        TreeNode parentNode, string filePath,
        Dictionary<string, List<string>> fileRelationships, HashSet<string> processedFiles)
    {
        if (processedFiles.Contains(filePath) || !fileRelationships.TryGetValue(filePath, out var dependencies))
            return;

        processedFiles.Add(filePath); foreach (var dependency in dependencies)
        {
            if (File.Exists(dependency) && !processedFiles.Contains(dependency))
            {
                var dependencyName = Path.GetFileName(dependency);
                // Add cyan color for dependency nodes
                var dependencyText = HyperLink.For(dependency, dependencyName, new Style(Color.Cyan1));
                var childNode = parentNode.AddNode(dependencyText);

                // Continue recursively
                BuildReferenceTreeNodeRecursive(childNode, dependency, fileRelationships, processedFiles);
            }
        }
    }
}
