using Spectre.Console;
using Spectre.Console.Extensions;

namespace MergeIncludes.TreeBuilders;

/// <summary>
/// Utility class for building folder trees that align with reference trees line-by-line
/// Uses spacer nodes to ensure folder references align with corresponding files
/// </summary>
public static class AlignedFolderTreeBuilder
{
	/// <summary>
	/// Gets a value indicating whether we're running in Windows Terminal.
	/// </summary>
	private static bool IsWindowsTerminal => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION"));

	/// <summary>
	/// Create an aligned folder tree that matches the structure of a reference tree
	/// </summary>
	public static TreeMinimalWidth FromDependencies(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// First, build the reference structure to understand the alignment requirements
		var referenceStructure = BuildReferenceStructure(rootFile, fileRelationships);

		// Then create the aligned folder tree
		return CreateAlignedFolderTree(rootFile, referenceStructure);
	}

	/// <summary>
	/// Represents a node in the reference structure for alignment
	/// </summary>
	private class ReferenceNode
	{
		public string FilePath { get; set; } = "";
		public string FileName { get; set; } = "";
		public DirectoryInfo Directory { get; set; } = null!;
		public List<ReferenceNode> Children { get; set; } = [];
		public int Level { get; set; } = 0;
	}

	/// <summary>
	/// Build the reference structure to understand what needs to align
	/// </summary>
	private static List<ReferenceNode> BuildReferenceStructure(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		var structure = new List<ReferenceNode>();
		var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		// Add root file
		var rootNode = new ReferenceNode
		{
			FilePath = rootFile.FullName,
			FileName = rootFile.Name,
			Directory = rootFile.Directory!,
			Level = 0
		};
		structure.Add(rootNode);

		// Recursively build the structure including ALL references (duplicates)
		BuildReferenceStructureRecursive(rootNode, fileRelationships, visitedPaths);

		// Flatten to a linear list for alignment
		var flatList = new List<ReferenceNode>();
		FlattenReferenceStructure(structure, flatList);

		return flatList;
	}    /// <summary>
		 /// Recursively build the reference structure including all references and duplicates
		 /// </summary>
	private static void BuildReferenceStructureRecursive(
		ReferenceNode parentNode,
		Dictionary<string, List<string>> fileRelationships,
		HashSet<string> visitedPaths)
	{
		// Check for circular references to prevent infinite recursion
		if (visitedPaths.Contains(parentNode.FilePath))
			return;

		if (!fileRelationships.TryGetValue(parentNode.FilePath, out var dependencies))
			return;

		// Add to visited paths for this branch
		visitedPaths.Add(parentNode.FilePath);

		foreach (var dependency in dependencies)
		{
			// Skip self-references to prevent root file duplication
			if (string.Equals(dependency, parentNode.FilePath, StringComparison.OrdinalIgnoreCase))
				continue;

			if (File.Exists(dependency))
			{
				var dependencyFile = new FileInfo(dependency);
				var childNode = new ReferenceNode
				{
					FilePath = dependency,
					FileName = dependencyFile.Name,
					Directory = dependencyFile.Directory!,
					Level = parentNode.Level + 1
				};

				parentNode.Children.Add(childNode);

				// Recursively process ALL references (including duplicates)
				BuildReferenceStructureRecursive(
					childNode,
					fileRelationships,
					new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
			}
		}

		// Remove from visited paths when leaving this branch
		visitedPaths.Remove(parentNode.FilePath);
	}

	/// <summary>
	/// Flatten the reference structure to a linear list for alignment
	/// </summary>
	private static void FlattenReferenceStructure(List<ReferenceNode> nodes, List<ReferenceNode> flatList)
	{
		foreach (var node in nodes)
		{
			flatList.Add(node);
			FlattenReferenceStructure(node.Children, flatList);
		}
	}    /// <summary>
		 /// Create an aligned folder tree based on the reference structure
		 /// </summary>
	private static TreeMinimalWidth CreateAlignedFolderTree(FileInfo rootFile, List<ReferenceNode> referenceStructure)
	{
		// Create root folder - blue color, no bold
		var rootFolderName = $"📁 {rootFile.Directory!.Name}";
		var rootStyle = IsWindowsTerminal
			? new Style(foreground: Color.Blue, link: rootFile.Directory.FullName)
			: new Style(foreground: Color.Blue);

		var rootText = new Text(rootFolderName, rootStyle);
		var tree = new TreeMinimalWidth(rootText);

		// Process each reference node to maintain 1:1 alignment
		ProcessReferenceNodesForAlignment(tree, rootFile.Directory, referenceStructure.Skip(1));

		return tree;
	}

	/// <summary>
	/// Process reference nodes to maintain 1:1 alignment with smart hierarchical structure
	/// Create hierarchy when files naturally nest in same/sub folders, flatten when they don't
	/// </summary>
	private static void ProcessReferenceNodesForAlignment(
		TreeMinimalWidth tree,
		DirectoryInfo baseDirectory,
		IEnumerable<ReferenceNode> referenceNodes)
	{
		var nodeList = referenceNodes is IReadOnlyList<ReferenceNode> l ? l : referenceNodes.ToArray();
		if (nodeList.Count == 0) return;

		string? lastDirectoryPath = null;
		IHasTreeNodes? currentFolderNode = null;

		for (int i = 0; i < nodeList.Count; i++)
		{
			var referenceNode = nodeList[i];
			var currentDirectoryPath = referenceNode.Directory.FullName;

			// Check if we should create hierarchy: consecutive files from the same folder
			bool shouldBeChild = ShouldBeChildOfPreviousFolder(nodeList, i, currentDirectoryPath);

			if (shouldBeChild && currentFolderNode != null)
			{
				// This file should be a child of the previous folder
				var spacer = new Text("", new Style(foreground: Color.Default));
				currentFolderNode.AddNode(spacer);
			}
			else
			{
				// This file starts a new folder or goes back to root
				if (currentDirectoryPath.Equals(baseDirectory.FullName, StringComparison.OrdinalIgnoreCase))
				{
					// Back to root directory - show an empty spacer
					var spacer = new Text("", new Style(foreground: Color.Default));
					tree.AddNode(spacer);
					currentFolderNode = null;
				}
				else if (!string.Equals(currentDirectoryPath, lastDirectoryPath, StringComparison.OrdinalIgnoreCase))
				{
					// Different directory - show the folder name
					var folderName = GetFolderDisplayName(baseDirectory, referenceNode.Directory);
					var folderStyle = IsWindowsTerminal
						? new Style(foreground: Color.Green, link: referenceNode.Directory.FullName)
						: new Style(foreground: Color.Green);

					var folderText = new Text(folderName, folderStyle);
					currentFolderNode = tree.AddNode(folderText);
				}
				else
				{
					// Same directory as previous file - show an empty spacer
					var spacer = new Text("", new Style(foreground: Color.Default));
					tree.AddNode(spacer);
				}
			}

			lastDirectoryPath = currentDirectoryPath;
		}
	}

	/// <summary>
	/// Determine if this file should be a child of the previous folder
	/// Simple rule: if consecutive files are from the same folder, make them children
	/// </summary>
	private static bool ShouldBeChildOfPreviousFolder(IReadOnlyList<ReferenceNode> nodeList, int currentIndex, string currentDirectoryPath)
	{
		if (currentIndex == 0) return false; // First file can't be a child		// Look at the previous file
		var previousNode = nodeList[currentIndex - 1];
		var previousDirectoryPath = previousNode.Directory.FullName;

		// If same directory as previous file, this should be a child
		return string.Equals(currentDirectoryPath, previousDirectoryPath, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Get the display name for a folder, showing relative path if outside base directory
	/// </summary>
	private static string GetFolderDisplayName(DirectoryInfo baseDirectory, DirectoryInfo folder)
	{
		try
		{
			// Check if the folder is within the base directory tree
			var relativePath = Path.GetRelativePath(baseDirectory.FullName, folder.FullName);

			// If relativePath starts with "..", it's outside the base directory tree
			if (relativePath.StartsWith(".."))
			{
				// Show the relative path with folder icon, normalize to forward slashes
				var normalizedPath = relativePath.Replace('\\', '/');
				return $"📁 {normalizedPath}";
			}
			else
			{
				// It's within the base directory tree, just show the folder name
				return $"📁 {folder.Name}";
			}
		}
		catch
		{
			// Fallback to just the folder name if path operations fail
			return $"📁 {folder.Name}";
		}
	}
}
