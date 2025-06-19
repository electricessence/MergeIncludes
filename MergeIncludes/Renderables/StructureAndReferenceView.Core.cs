using MergeIncludes.TreeBuilders;
using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;

namespace MergeIncludes.Renderables;

/// <summary>
/// Core functionality for the StructureAndReferenceView - renders the side-by-side folder structure and reference trees view using Tree widgets
/// </summary>
public sealed partial class StructureAndReferenceView : IRenderable
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

		_content = CreateTwoColumnTreeTable(rootFile, fileRelationships);
	}

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
	}

	/// <summary>
	/// Creates the two-column table layout with folder structure and reference trees
	/// </summary>
	private static Table CreateTwoColumnTreeTable(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Create the left column: folder structure tree using aligned builder
		var folderTree = AlignedFolderTreeBuilder.FromDependencies(rootFile, fileRelationships);
		// Create the middle separator column with just "/"		var separator = new Text(" / ", new Style(Color.Grey));
		// Create the separator for the middle column
		var separator = new Text(" / ", new Style(Color.Grey));

		// Create the right column: reference tree using SimpleReferenceTreeBuilder
		var referenceTree = SimpleReferenceTreeBuilder.BuildReferenceTree(rootFile.FullName, fileRelationships);

		// Create a table with three columns for side-by-side display
		var table = new Table()
			.Border(TableBorder.None)
			.HideHeaders()
			.Collapse()  // Makes table take up minimal width
			.AddColumn(new TableColumn("Folder") { NoWrap = true, Padding = new Padding(0, 0, 0, 0) })  // No padding
			.AddColumn(new TableColumn("Separator") { NoWrap = true, Width = 3, Padding = new Padding(0, 0, 0, 0) })  // Exact width for " / "
			.AddColumn(new TableColumn("References") { NoWrap = true, Padding = new Padding(0, 0, 0, 0) });  // No padding

		// Add the trees with separator
		table.AddRow(folderTree, separator, referenceTree);

		return table;
	}
}
