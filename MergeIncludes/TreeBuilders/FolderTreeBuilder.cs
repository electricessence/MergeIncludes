using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;

namespace MergeIncludes.TreeBuilders;

/// <summary>
/// Utility class for building folder structure trees using native Spectre.Console Tree
/// </summary>
public static class FolderTreeBuilder
{
	/// <summary>
	/// Gets a value indicating whether we're running in Windows Terminal.
	/// </summary>
	private static bool IsWindowsTerminal => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION"));

	/// <summary>
	/// Create a folder tree from a root file and its dependencies
	/// </summary>
	public static IRenderable FromDependencies(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		var allFiles = new List<FileInfo> { rootFile };
		CollectAllFilesRecursive(rootFile.FullName, fileRelationships, allFiles);
		return CreateFolderTree(rootFile.Directory!, allFiles);
	}

	/// <summary>
	/// Create a folder tree for the given files
	/// </summary>
	public static IRenderable Create(DirectoryInfo baseDirectory, IEnumerable<FileInfo> files)
	{
		return CreateFolderTree(baseDirectory, files);
	}

	/// <summary>
	/// Create a folder tree for the given files
	/// </summary>
	private static TreeMinimalWidth CreateFolderTree(DirectoryInfo baseDirectory, IEnumerable<FileInfo> files)
	{
		// Create root folder, only add link in Windows Terminal
		var rootFolderName = $"📁 {baseDirectory.Name}";
		var rootStyle = IsWindowsTerminal
			? new Style(foreground: Color.Blue, decoration: Decoration.Bold, link: baseDirectory.FullName)
			: new Style(foreground: Color.Blue, decoration: Decoration.Bold);

		var rootText = new Text(rootFolderName, rootStyle);

		var tree = new TreeMinimalWidth(rootText);

		// Group files by their directories
		var filesByDirectory = files.GroupBy(f => f.Directory!.FullName).ToList();

		foreach (var directoryGroup in filesByDirectory)
		{
			var directory = new DirectoryInfo(directoryGroup.Key);
			var relativePath = Path.GetRelativePath(baseDirectory.FullName, directory.FullName);

			if (relativePath == "." || string.IsNullOrEmpty(relativePath))
			{
				// Files in root directory - add directly
				foreach (var file in directoryGroup.OrderBy(f => f.Name))
				{
					// Only add link in Windows Terminal
					var fileStyle = IsWindowsTerminal
						? new Style(foreground: Color.Green, link: file.FullName)
						: new Style(foreground: Color.Green);

					var fileText = new Text(file.Name, fileStyle);
					tree.AddNode(fileText);
				}
			}
			else
			{
				// Files in subdirectory - add folder then files with folder icon
				var folderName = $"📁 {directory.Name}";

				// Only add link in Windows Terminal
				var folderStyle = IsWindowsTerminal
					? new Style(foreground: Color.Cyan1, decoration: Decoration.Bold, link: directory.FullName)
					: new Style(foreground: Color.Cyan1, decoration: Decoration.Bold);

				var folderText = new Text(folderName, folderStyle);
				var folderNode = tree.AddNode(folderText);

				foreach (var file in directoryGroup.OrderBy(f => f.Name))
				{
					// Only add link in Windows Terminal
					var fileStyle = IsWindowsTerminal
						? new Style(foreground: Color.Green, link: file.FullName)
						: new Style(foreground: Color.Green);

					var fileText = new Text(file.Name, fileStyle);
					folderNode.AddNode(fileText);
				}
			}
		}

		return tree;
	}

	private static void CollectAllFilesRecursive(string filePath, Dictionary<string, List<string>> fileRelationships, List<FileInfo> allFiles)
	{
		if (!fileRelationships.TryGetValue(filePath, out var children))
			return;

		foreach (var childPath in children)
		{
			if (File.Exists(childPath) && !allFiles.Any(f => f.FullName.Equals(childPath, StringComparison.OrdinalIgnoreCase)))
			{
				allFiles.Add(new FileInfo(childPath));
				CollectAllFilesRecursive(childPath, fileRelationships, allFiles);
			}
		}
	}
}
