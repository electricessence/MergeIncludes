using Spectre.Console;

namespace MergeIncludes;

/// <summary>
/// FullPath display mode - simple list of files with full paths
/// </summary>
public sealed partial class CombineCommand
{
	/// <summary>
	/// Displays a simple list of all files with their full paths in inclusion order
	/// This matches the original behavior before display modes were introduced
	/// </summary>
	private void DisplayFullPathTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		try
		{
			// Collect all files in inclusion order
			var allFiles = new List<string>();
			var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			// Add root file first
			allFiles.Add(rootFile.FullName);
			visitedFiles.Add(rootFile.FullName);

			// Recursively collect all included files
			CollectAllFilesInOrder(rootFile.FullName, fileRelationships, allFiles, visitedFiles);

			// Display files in a panel with consistent path formatting (forward slashes like success panel)
			var fileLines = allFiles.Select(filePath => new TextPath(filePath)).ToArray();
			var panel = new Panel(new Rows(fileLines))
			{
				Header = new PanelHeader("[white]Files included in merge:[/]"),
				Border = BoxBorder.Rounded,
				Padding = new Padding(1, 0, 1, 0)
			};
			_console.Write(panel);
		}
		catch (Exception ex)
		{
			_console.MarkupLine($"[red]Error displaying full paths:[/] {ex.Message}");
		}
	}

	/// <summary>
	/// Recursively collects all files in inclusion order
	/// </summary>
	private static void CollectAllFilesInOrder(string currentFile, Dictionary<string, List<string>> fileRelationships,
		List<string> allFiles, HashSet<string> visitedFiles)
	{
		if (fileRelationships.TryGetValue(currentFile, out var children))
		{
			foreach (var childPath in children)
			{
				if (!visitedFiles.Contains(childPath))
				{
					allFiles.Add(childPath);
					visitedFiles.Add(childPath);

					// Recursively collect children
					CollectAllFilesInOrder(childPath, fileRelationships, allFiles, visitedFiles);
				}
			}
		}
	}
}
