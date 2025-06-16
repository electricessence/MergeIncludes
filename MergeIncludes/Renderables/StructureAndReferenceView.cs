using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;

namespace MergeIncludes.Renderables;

/// <summary>
/// Renders the side-by-side folder structure and reference trees view using Tree widgets
/// </summary>
public sealed class StructureAndReferenceView : IRenderable
{
	private readonly IRenderable _content;

	/// <summary>
	/// Initializes a new instance of the StructureAndReferenceView class
	/// </summary>
	/// <param name="rootFile">The root file being processed</param>
	/// <param name="fileRelationships">Dictionary mapping files to their dependencies</param>
	public StructureAndReferenceView(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		if (rootFile == null) throw new ArgumentNullException(nameof(rootFile));
		if (fileRelationships == null) throw new ArgumentNullException(nameof(fileRelationships));

		_content = CreateTwoColumnTreeTable(rootFile, fileRelationships);	}

	/// <inheritdoc/>
	public Measurement Measure(RenderOptions options, int maxWidth)
	{
		// Delegate to the internal content for measurement
		return _content.Measure(options, maxWidth);
	}

	/// <inheritdoc/>
	public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
	{
		// Delegate to the internal content for rendering
		return _content.Render(options, maxWidth);
	}	private static IRenderable CreateTwoColumnTreeTable(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Create the left column: folder structure tree
		var folderTree = CreateFolderStructureTree(rootFile);

		// Create the middle separator column with just "/"
		var separator = new Text("/", new Style(Color.Grey));

		// Create the right column: reference tree
		var referenceTree = CreateReferenceTree(rootFile, fileRelationships);

		// Create a table with three columns for side-by-side display
		var table = new Table()
			.Border(TableBorder.None)
			.HideHeaders()
			.Collapse()  // Makes table take up minimal width
			.AddColumn(new TableColumn("Folder") { NoWrap = true, Padding = new Padding(0, 0, 1, 0) })  // Small right padding
			.AddColumn(new TableColumn("Separator") { NoWrap = true, Padding = new Padding(0, 0, 1, 0) })  // Small padding on both sides
			.AddColumn(new TableColumn("References") { NoWrap = true, Padding = new Padding(0, 0, 0, 0) });

		// Add the trees with separator
		table.AddRow(folderTree, separator, referenceTree);

		return table;
	}private static IRenderable CreateFolderStructureTree(FileInfo rootFile)
	{
		// Create a simple folder tree showing the file location
		return TreeBuilders.FolderTreeBuilder.Create(rootFile.Directory!, [rootFile]);
	}	private static IRenderable CreateReferenceTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Create root node with yellow color (not bold)
		// HyperLink.For handles Windows Terminal detection internally
		var rootText = HyperLink.For(rootFile, new Style(Color.Yellow));
		var tree = new TreeMinimalWidth(rootText);
		var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		BuildReferenceTreeRecursive(tree, rootFile.FullName, fileRelationships, processedFiles);

		return tree;
	}
	/// <summary>
	/// Recursively builds the reference tree hierarchy
	/// </summary>
	private static void BuildReferenceTreeRecursive(
		IHasTreeNodes tree, string filePath,
		Dictionary<string, List<string>> fileRelationships,
		HashSet<string> processedFiles)
	{
		if (processedFiles.Contains(filePath) || !fileRelationships.TryGetValue(filePath, out var dependencies))
			return;

		processedFiles.Add(filePath);

		foreach (var dependency in dependencies)
		{
			if (File.Exists(dependency) && !processedFiles.Contains(dependency))
			{
				var dependencyFile = new FileInfo(dependency);
				// HyperLink.For handles Windows Terminal detection internally
				var dependencyText = HyperLink.For(dependencyFile, new Style(Color.Cyan1));
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

		processedFiles.Add(filePath);

		foreach (var dependency in dependencies)
		{
			if (File.Exists(dependency) && !processedFiles.Contains(dependency))
			{
				var dependencyFile = new FileInfo(dependency);
				// HyperLink.For handles Windows Terminal detection internally
				var dependencyText = HyperLink.For(dependencyFile, new Style(Color.Cyan1));
				var childNode = parentNode.AddNode(dependencyText);

				// Continue recursively
				BuildReferenceTreeNodeRecursive(childNode, dependency, fileRelationships, processedFiles);
			}
		}
	}
}
