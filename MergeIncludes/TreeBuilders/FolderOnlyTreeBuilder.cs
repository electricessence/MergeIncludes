using Spectre.Console;
using Spectre.Console.Extensions;

namespace MergeIncludes.TreeBuilders;

/// <summary>
/// Utility class for building folder-only structure trees (no files shown) using native Spectre.Console Tree
/// This provides a cleaner view that correlates better with reference trees.
/// </summary>
public static class FolderOnlyTreeBuilder
{
	/// <summary>
	/// Gets a value indicating whether we're running in Windows Terminal.
	/// </summary>
	private static bool IsWindowsTerminal => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION"));

	/// <summary>
	/// Create a folder-only tree from a root file and its dependencies
	/// </summary>
	public static TreeMinimalWidth FromDependencies(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		var allFiles = new List<FileInfo> { rootFile };
		CollectAllFilesRecursive(rootFile.FullName, fileRelationships, allFiles);
		return CreateFolderOnlyTree(rootFile.Directory!, allFiles);
	}

	/// <summary>
	/// Create a folder-only tree for the given files
	/// </summary>
	public static TreeMinimalWidth Create(DirectoryInfo baseDirectory, IEnumerable<FileInfo> files)
	{
		return CreateFolderOnlyTree(baseDirectory, files);
	}

	/// <summary>
	/// Create a folder-only tree for the given files, showing only directory structure
	/// </summary>
	private static TreeMinimalWidth CreateFolderOnlyTree(DirectoryInfo baseDirectory, IEnumerable<FileInfo> files)
	{
		// Create root folder, only add link in Windows Terminal
		var rootFolderName = $"📁 {baseDirectory.Name}";
		var rootStyle = IsWindowsTerminal
			? new Style(foreground: Color.Blue, decoration: Decoration.Bold, link: baseDirectory.FullName)
			: new Style(foreground: Color.Blue, decoration: Decoration.Bold);

		var rootText = new Text(rootFolderName, rootStyle);
		var tree = new TreeMinimalWidth(rootText);

		// Collect all unique directories referenced by the files
		var allDirectories = files
			.Select(f => f.Directory!)
			.Where(d => d != null)
			.Distinct(new DirectoryInfoComparer())
			.ToList();

		// Build a hierarchy of directories
		var directoryHierarchy = BuildDirectoryHierarchy(baseDirectory, allDirectories);

		// Add directories to tree recursively
		AddDirectoriesToTree(tree, baseDirectory, directoryHierarchy);

		return tree;
	}

	/// <summary>
	/// Build a hierarchy map of directories
	/// </summary>
	private static Dictionary<DirectoryInfo, List<DirectoryInfo>> BuildDirectoryHierarchy(
		DirectoryInfo baseDirectory,
		List<DirectoryInfo> allDirectories)
	{
		var hierarchy = new Dictionary<DirectoryInfo, List<DirectoryInfo>>(new DirectoryInfoComparer());

		foreach (var directory in allDirectories)
		{
			// Skip the base directory itself
			if (directory.FullName.Equals(baseDirectory.FullName, StringComparison.OrdinalIgnoreCase))
				continue;

			// Find the parent directory
			var parent = FindParentInHierarchy(baseDirectory, directory, allDirectories);

			if (!hierarchy.ContainsKey(parent))
				hierarchy[parent] = [];

			hierarchy[parent].Add(directory);
		}

		// Sort children by name
		foreach (var kvp in hierarchy)
		{
			kvp.Value.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
		}

		return hierarchy;
	}

	/// <summary>
	/// Find the immediate parent directory in our hierarchy
	/// </summary>
	private static DirectoryInfo FindParentInHierarchy(
		DirectoryInfo baseDirectory,
		DirectoryInfo directory,
		List<DirectoryInfo> allDirectories)
	{
		var current = directory.Parent;

		while (current != null)
		{
			// If we reached the base directory, it's the parent
			if (current.FullName.Equals(baseDirectory.FullName, StringComparison.OrdinalIgnoreCase))
				return baseDirectory;

			// If this directory is in our list, it's the parent
			if (allDirectories.Any(d => d.FullName.Equals(current.FullName, StringComparison.OrdinalIgnoreCase)))
				return current;

			current = current.Parent;
		}

		// Fallback to base directory
		return baseDirectory;
	}

	/// <summary>
	/// Recursively add directories to the tree
	/// </summary>
	private static void AddDirectoriesToTree(
		TreeMinimalWidth tree,
		DirectoryInfo currentDirectory,
		Dictionary<DirectoryInfo, List<DirectoryInfo>> hierarchy)
	{
		if (!hierarchy.TryGetValue(currentDirectory, out List<DirectoryInfo>? value))
			return;

		foreach (var childDirectory in value)
		{
			// Create folder node
			var folderName = $"📁 {childDirectory.Name}";

			// Only add link in Windows Terminal
			var folderStyle = IsWindowsTerminal
				? new Style(foreground: Color.Cyan1, decoration: Decoration.Bold, link: childDirectory.FullName)
				: new Style(foreground: Color.Cyan1, decoration: Decoration.Bold);

			var folderText = new Text(folderName, folderStyle);
			var folderNode = tree.AddNode(folderText);

			// Recursively add child directories
			AddChildDirectoriesToNode(folderNode, childDirectory, hierarchy);
		}
	}

	/// <summary>
	/// Recursively add child directories to a specific node
	/// </summary>
	private static void AddChildDirectoriesToNode(
		TreeNode folderNode,
		DirectoryInfo currentDirectory,
		Dictionary<DirectoryInfo, List<DirectoryInfo>> hierarchy)
	{
		if (!hierarchy.TryGetValue(currentDirectory, out List<DirectoryInfo>? value))
			return;

		foreach (var childDirectory in value)
		{
			// Create folder node
			var folderName = $"📁 {childDirectory.Name}";

			// Only add link in Windows Terminal
			var folderStyle = IsWindowsTerminal
				? new Style(foreground: Color.Cyan1, decoration: Decoration.Bold, link: childDirectory.FullName)
				: new Style(foreground: Color.Cyan1, decoration: Decoration.Bold);

			var folderText = new Text(folderName, folderStyle);
			var childFolderNode = folderNode.AddNode(folderText);

			// Recursively add child directories
			AddChildDirectoriesToNode(childFolderNode, childDirectory, hierarchy);
		}
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

	/// <summary>
	/// Custom comparer for DirectoryInfo objects
	/// </summary>
	private class DirectoryInfoComparer : IEqualityComparer<DirectoryInfo>
	{
		public bool Equals(DirectoryInfo? x, DirectoryInfo? y)
		{
			if (x == null && y == null) return true;
			if (x == null || y == null) return false;
			return string.Equals(x.FullName, y.FullName, StringComparison.OrdinalIgnoreCase);
		}

		public int GetHashCode(DirectoryInfo obj)
		{
			return obj.FullName.ToLowerInvariant().GetHashCode();
		}
	}
}
