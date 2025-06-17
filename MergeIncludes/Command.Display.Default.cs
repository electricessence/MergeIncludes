using MergeIncludes.Renderables;
using Spectre.Console;
using Spectre.Console.Extensions;

namespace MergeIncludes;

/// <summary>
/// Default display mode - side-by-side folder structure and reference trees
/// </summary>
partial class CombineCommand
{
	/// <summary>
	/// Displays two trees side by side: folder structure on left, reference structure on right
	/// </summary>
	private void DisplayDefaultTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Get the base directory of the root file
		var baseDirectory = rootFile.Directory
			?? throw new InvalidOperationException("Root file directory cannot be null");
		// Create the root path display using LinkableTextPath with execution-relative path
		// The display path is relative, but the link target is still the full path for proper linking
		var relativePath = GetExecutionRelativeFilePath(rootFile);
		var rootPath = new LinkableTextPath(relativePath, baseDirectory.FullName)
			.RootStyle(Color.Blue)
			.SeparatorStyle(Color.Grey)
			.StemStyle(Color.DarkGreen)
			.LeafStyle(Color.Yellow);

		// Create the structure and reference view (content below the HR)
		var structureAndReferenceView
			= new StructureAndReferenceView(rootFile, fileRelationships);

		_console.Write(new Panel(rootPath)
		{
			Header = new PanelHeader("Root File"),
			Border = BoxBorder.Rounded,
			Padding = new Padding(1, 0, 1, 0),
			Expand = false
		});

		// Create panel with the content
		_console.Write(new Panel(structureAndReferenceView)
		{
			Header = new PanelHeader("Structure"),
			Border = BoxBorder.Rounded,
			Padding = new Padding(1, 0, 2, 0),
			Expand = false
		});
	}
}
