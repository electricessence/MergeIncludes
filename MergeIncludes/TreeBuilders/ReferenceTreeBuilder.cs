using Spectre.Console;

namespace MergeIncludes.TreeBuilders;

/// <summary>
/// Utility class for building reference dependency trees using native Spectre.Console Tree
/// </summary>
public static class ReferenceTreeBuilder
{    /// <summary>
	 /// Create a reference tree with clickable file links using Style.link
	 /// </summary>
	public static Tree Create(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Create root file as clickable link using Style.link
		var rootText = new Text(rootFile.Name, new Style(foreground: Color.Yellow, decoration: Decoration.Bold, link: rootFile.FullName));

		var tree = new Tree(rootText);
		var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		// Build the dependency tree recursively
		BuildTreeRecursive(tree, rootFile.FullName, fileRelationships, processedFiles, 0);

		return tree;
	}    /// <summary>
		 /// Recursively builds the reference tree by adding child nodes
		 /// </summary>
	private static void BuildTreeRecursive(Tree tree, string filePath,
		Dictionary<string, List<string>> fileRelationships, HashSet<string> processedFiles, int depth)
	{
		if (processedFiles.Contains(filePath) || !fileRelationships.TryGetValue(filePath, out var children))
			return;

		processedFiles.Add(filePath);

		foreach (var childPath in children)
		{
			if (File.Exists(childPath) && !processedFiles.Contains(childPath))
			{
				var childFile = new FileInfo(childPath);
				var color = depth == 0 ? Color.Lime : Color.Cyan1;
				var childText = new Text(childFile.Name, new Style(foreground: color, link: childPath));

				var childNode = tree.AddNode(childText);

				// Recursively build from this child node
				BuildTreeNodeRecursive(childNode, childPath, fileRelationships, processedFiles, depth + 1);
			}
		}
	}    /// <summary>
		 /// Recursively builds child nodes in the reference tree
		 /// </summary>
	private static void BuildTreeNodeRecursive(TreeNode parentNode, string filePath,
		Dictionary<string, List<string>> fileRelationships, HashSet<string> processedFiles, int depth)
	{
		if (processedFiles.Contains(filePath) || !fileRelationships.TryGetValue(filePath, out var children))
			return;

		processedFiles.Add(filePath);

		foreach (var childPath in children)
		{
			if (File.Exists(childPath) && !processedFiles.Contains(childPath))
			{
				var childFile = new FileInfo(childPath);
				var color = depth <= 1 ? Color.Lime : Color.Cyan1;
				var childText = new Text(childFile.Name, new Style(foreground: color, link: childPath));

				var childNode = parentNode.AddNode(childText);

				// Continue recursively
				BuildTreeNodeRecursive(childNode, childPath, fileRelationships, processedFiles, depth + 1);
			}
		}
	}
}
