using MergeIncludes.Renderables;
using Spectre.Console;

namespace MergeIncludes;

/// <summary>
/// Display utilities for the CombineCommand
/// </summary>
partial class CombineCommand
{
	/// <summary>
	/// Creates a basic reference tree with proper hierarchy
	/// </summary>
	private static Tree CreateBasicReferenceTree(
		FileInfo rootFile,
		Dictionary<string, List<string>> fileRelationships)
	{
		var rootMarkup = HyperLink.For(rootFile);
		var tree = new Tree(rootMarkup);
		var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		BuildReferenceTreeRecursive(tree, rootFile.FullName, fileRelationships, processedFiles);

		return tree;
	}

	/// <summary>
	/// Recursively builds the reference tree hierarchy
	/// </summary>
	private static void BuildReferenceListRecursive(
		Tree tree, string filePath,
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
				var dependencyName = Path.GetFileName(dependency);
				var dependencyMarkup = HyperLink.For(dependency, dependencyName);
				var childNode = tree.AddNode(dependencyMarkup);

				// Recursively build dependencies of this dependency
				BuildReferenceTreeNodeRecursive(childNode, dependency, fileRelationships, processedFiles);
			}
		}
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

		processedFiles.Add(filePath);
		foreach (var dependency in dependencies)
		{
			if (File.Exists(dependency) && !processedFiles.Contains(dependency))
			{
				var dependencyName = Path.GetFileName(dependency);
				var dependencyMarkup = HyperLink.For(dependency, dependencyName);
				var childNode = tree.AddNode(dependencyMarkup);

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
				var dependencyName = Path.GetFileName(dependency);
				var dependencyMarkup = HyperLink.For(dependency, dependencyName);
				var childNode = parentNode.AddNode(dependencyMarkup);

				// Continue recursively
				BuildReferenceTreeNodeRecursive(childNode, dependency, fileRelationships, processedFiles);
			}
		}
	}
}
