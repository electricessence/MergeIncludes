using Spectre.Console;
using Spectre.Console.Extensions;

namespace MergeIncludes.Renderables;

public sealed partial class StructureAndReferenceView
{
	/// <summary>
	/// Creates the reference tree showing file inclusion hierarchy
	/// </summary>
	private static TreeMinimalWidth CreateReferenceTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Create root node with yellow color (not bold)
		var rootText = HyperLink.For(rootFile, new Style(Color.Yellow));
		var tree = new TreeMinimalWidth(rootText);

		// Find repeated files first
		var repeatedFiles = FindRepeatedFiles(rootFile, fileRelationships);

		// Track which files we've seen and assign them unique IDs only for repeated files
		var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		var nextId = 1; // Start from 1, not 0

		// Don't assign ID to root file - it's not part of the reference tree display

		// Track visited paths to prevent infinite recursion due to circular references
		var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		
		BuildCompleteReferenceTreeRecursive(tree, rootFile.FullName, fileRelationships, fileIds, ref nextId, visitedPaths, repeatedFiles);

		return tree;
	}

	/// <summary>
	/// Recursively builds the complete reference tree hierarchy with proper numbering for duplicates only
	/// </summary>
	private static void BuildCompleteReferenceTreeRecursive(
		IHasTreeNodes tree, string filePath,
		Dictionary<string, List<string>> fileRelationships,
		Dictionary<string, int> fileIds,
		ref int nextId,
		HashSet<string> visitedPaths,
		HashSet<string> repeatedFiles)
	{
		// Add this path to the visited set for this branch
		visitedPaths.Add(filePath);

		if (!fileRelationships.TryGetValue(filePath, out var dependencies))
		{
			// Remove from visited before returning
			visitedPaths.Remove(filePath);
			return;
		}

		foreach (var dependency in dependencies)
		{
			// Skip self-references to prevent root file duplication  
			if (string.Equals(dependency, filePath, StringComparison.OrdinalIgnoreCase))
				continue;

			if (File.Exists(dependency))
			{
				var dependencyFile = new FileInfo(dependency);

				// Check if this is the first time we've seen this file
				bool isFirstOccurrence = !fileIds.ContainsKey(dependency);
				bool isRepeated = repeatedFiles.Contains(dependency);

				// Check for circular references - if we've visited this path in the current branch
				bool isCircularReference = visitedPaths.Contains(dependency);

				// Only assign ID for repeated files
				if (isRepeated && isFirstOccurrence)
				{
					// First occurrence of a repeated file: assign new ID
					fileIds[dependency] = nextId++;
				}

				var fileName = dependencyFile.Name;
				var displayText = isRepeated && fileIds.TryGetValue(dependency, out var fileId) 
					? $"{fileName} [{fileId}]" 
					: fileName;

				if (isCircularReference)
				{
					// Circular reference: show in red with warning icon
					displayText += " ⚠";
					var dependencyText = HyperLink.For(dependencyFile.FullName, displayText, new Style(Color.Red));
					tree.AddNode(dependencyText);
					// Do NOT recurse for circular references
				}
				else if (isFirstOccurrence)
				{
					// First occurrence: use cyan color
					var dependencyText = HyperLink.For(dependencyFile.FullName, displayText, new Style(Color.Cyan1));
					var childNode = tree.AddNode(dependencyText);

					// Recursively build dependencies
					BuildCompleteReferenceTreeNodeRecursive(
						childNode,
						dependency,
						fileRelationships,
						fileIds,
						ref nextId,
						new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase),
						repeatedFiles);
				}
				else
				{
					// Subsequent occurrence: use existing ID, gray color, but STILL show children
					var dependencyText = HyperLink.For(dependencyFile.FullName, displayText, new Style(Color.Grey));
					var childNode = tree.AddNode(dependencyText);

					// ALSO recurse for subsequent occurrences to show full tree
					BuildCompleteReferenceTreeNodeRecursive(
						childNode,
						dependency,
						fileRelationships,
						fileIds,
						ref nextId,
						new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase),
						repeatedFiles);
				}
			}
		}

		// Remove from visited before returning to allow the file to appear in other branches
		visitedPaths.Remove(filePath);
	}

	/// <summary>
	/// Recursively builds child nodes in the complete reference tree
	/// </summary>
	private static void BuildCompleteReferenceTreeNodeRecursive(
		TreeNode parentNode, string filePath,
		Dictionary<string, List<string>> fileRelationships,
		Dictionary<string, int> fileIds,
		ref int nextId,
		HashSet<string> visitedPaths,
		HashSet<string> repeatedFiles)
	{
		// Add this path to the visited set for this branch
		visitedPaths.Add(filePath);

		if (!fileRelationships.TryGetValue(filePath, out var dependencies))
		{
			// Remove from visited before returning
			visitedPaths.Remove(filePath);
			return;
		}

		foreach (var dependency in dependencies)
		{
			// Skip self-references to prevent root file duplication  
			if (string.Equals(dependency, filePath, StringComparison.OrdinalIgnoreCase))
				continue;

			if (File.Exists(dependency))
			{
				var dependencyFile = new FileInfo(dependency);

				// Check if this is the first time we've seen this file
				bool isFirstOccurrence = !fileIds.ContainsKey(dependency);
				bool isRepeated = repeatedFiles.Contains(dependency);

				// Check for circular references - if we've visited this path in the current branch
				bool isCircularReference = visitedPaths.Contains(dependency);

				// Only assign ID for repeated files
				if (isRepeated && isFirstOccurrence)
				{
					// First occurrence of a repeated file: assign new ID
					fileIds[dependency] = nextId++;
				}

				var fileName = dependencyFile.Name;
				var displayText = isRepeated && fileIds.TryGetValue(dependency, out var fileId) 
					? $"{fileName} [{fileId}]" 
					: fileName;

				if (isCircularReference)
				{
					// Circular reference: show in red with warning icon
					displayText += " ⚠";
					var dependencyText = HyperLink.For(dependencyFile.FullName, displayText, new Style(Color.Red));
					parentNode.AddNode(dependencyText);
					// Do NOT recurse for circular references
				}
				else if (isFirstOccurrence)
				{
					// First occurrence: use cyan color
					var dependencyText = HyperLink.For(dependencyFile.FullName, displayText, new Style(Color.Cyan1));
					var childNode = parentNode.AddNode(dependencyText);

					// Recursively build dependencies
					BuildCompleteReferenceTreeNodeRecursive(
						childNode,
						dependency,
						fileRelationships,
						fileIds,
						ref nextId,
						new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase),
						repeatedFiles);
				}
				else
				{
					// Subsequent occurrence: use existing ID, gray color, but STILL show children
					var dependencyText = HyperLink.For(dependencyFile.FullName, displayText, new Style(Color.Grey));
					var childNode = parentNode.AddNode(dependencyText);

					// ALSO recurse for subsequent occurrences to show full tree
					BuildCompleteReferenceTreeNodeRecursive(
						childNode,
						dependency,
						fileRelationships,
						fileIds,
						ref nextId,
						new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase),
						repeatedFiles);
				}
			}
		}

		// Remove from visited before returning to allow the file to appear in other branches
		visitedPaths.Remove(filePath);
	}

	/// <summary>
	/// Finds files that are referenced multiple times in the file tree
	/// </summary>
	private static HashSet<string> FindRepeatedFiles(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Count file references to find repeated files
		var fileOccurrences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		var repeatedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		// Use a queue for breadth-first traversal
		var queue = new Queue<string>();
		queue.Enqueue(rootFile.FullName);

		// Track visited files to avoid cycles
		var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			rootFile.FullName
		};

		while (queue.Count > 0)
		{
			var currentFile = queue.Dequeue();

			if (!fileRelationships.TryGetValue(currentFile, out var children))
				continue;

			foreach (var child in children)
			{
				// Skip self-references
				if (string.Equals(child, currentFile, StringComparison.OrdinalIgnoreCase))
					continue;

				// Count references
				if (!fileOccurrences.TryGetValue(child, out int count))
				{
					count = 0;
				}

				fileOccurrences[child] = count + 1;

				// If this is the second occurrence, mark it as repeated
				if (count + 1 > 1)
				{
					repeatedFiles.Add(child);
				}

				// Only visit this file once
				if (visited.Add(child))
				{
					queue.Enqueue(child);
				}
			}
		}

		return repeatedFiles;
	}
}
