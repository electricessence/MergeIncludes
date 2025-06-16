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

		// Create the root path display using LinkableTextPath
		// The path is used as both display and link target
		// Windows Terminal detection is handled internally in the LinkableTextPath.Render method
		var rootPath = new LinkableTextPath(baseDirectory.FullName, true)
			.RootStyle(Color.Blue)
			.SeparatorStyle(Color.Grey)
			.StemStyle(Color.DarkGreen)
			.LeafStyle(Color.Yellow);

		// Create the structure and reference view (content below the HR)
		var structureAndReferenceView
			= new StructureAndReferenceView(rootFile, fileRelationships);

		// Create content with header, separator, and the structure/reference view
		var content = new Rows(
			rootPath,
			new Rule() { Style = Color.Grey },
			structureAndReferenceView
		);

		// Create panel with the content
		var panel = new Panel(content)
		{
			Border = BoxBorder.Rounded,
			Padding = new Padding(1, 0, 1, 0),
			Expand = false
		};

		_console.Write(panel);
	}
}
