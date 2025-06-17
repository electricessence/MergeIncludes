using Spectre.Console;

namespace MergeIncludes.Services;

/// <summary>
/// Service for building side-by-side folder structure and reference trees
/// </summary>
public class DefaultTreeBuilder
{
	/// <summary>
	/// Builds side-by-side trees with aligned rows for folder structure and reference relationships
	/// </summary>
	public static (List<string> folderLines, List<string> referenceLines) BuildSideBySideTrees(
		FileInfo rootFile,
		Dictionary<string, List<string>> fileRelationships)
	{
		// Build reference tree lines first (this drives the structure)
		var referenceLines = BuildReferenceTreeLines(rootFile, fileRelationships);

		// Build aligned folder lines that correspond to each reference line
		var folderLines = BuildAlignedFolderLines(rootFile, fileRelationships, referenceLines);

		return (folderLines, referenceLines);
	}

	/// <summary>
	/// Collects all files in the order they appear in the dependency tree
	/// </summary>
	private static List<FileInfo> CollectAllFilesInOrder(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		var allFiles = new List<FileInfo>();
		var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		CollectFilesRecursive(rootFile.FullName, fileRelationships, allFiles, visitedPaths);
		return allFiles;
	}

	/// <summary>
	/// Recursively collects files in dependency order
	/// </summary>
	private static void CollectFilesRecursive(string filePath, Dictionary<string, List<string>> fileRelationships,
		List<FileInfo> allFiles, HashSet<string> visitedPaths)
	{
		if (visitedPaths.Contains(filePath))
			return;

		visitedPaths.Add(filePath);
		allFiles.Add(new FileInfo(filePath));

		if (fileRelationships.TryGetValue(filePath, out var children))
		{
			var validChildren = children.Where(path =>
				!string.IsNullOrEmpty(path) &&
				!string.Equals(path, filePath, StringComparison.OrdinalIgnoreCase)).ToList();

			foreach (var childPath in validChildren)
			{
				CollectFilesRecursive(childPath, fileRelationships, allFiles, visitedPaths);
			}
		}
	}
	/// <summary>
	/// Builds aligned folder lines that correspond to each reference line
	/// </summary>
	private static List<string> BuildAlignedFolderLines(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships, List<string> referenceLines)
	{
		var lines = new List<string>();

		// Collect all files and determine their folder positions
		var allFiles = CollectAllFilesInOrder(rootFile, fileRelationships);
		var baseDirectory = rootFile.Directory;

		if (baseDirectory == null)
		{
			return referenceLines.Select(_ => "").ToList();
		}

		// Pre-analyze the file structure to determine what connectors we need
		var hasRootFiles = allFiles.Any(f =>
			!f.FullName.Equals(rootFile.FullName, StringComparison.OrdinalIgnoreCase) &&
			f.Directory?.FullName.Equals(baseDirectory.FullName, StringComparison.OrdinalIgnoreCase) == true);

		var uniqueFolders = allFiles
			.Where(f => !f.FullName.Equals(rootFile.FullName, StringComparison.OrdinalIgnoreCase))
			.Select(f => GetRelativePath(baseDirectory.FullName, f.Directory?.FullName ?? ""))
			.Where(path => !string.IsNullOrEmpty(path) && path != ".")
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		// Determine if we have a single folder at root level (use └── instead of ├──)
		var isLastFolderAtRoot = !hasRootFiles && uniqueFolders.Count == 1;
		var totalFoldersAtRoot = uniqueFolders.Count;

		// Track which folders we've already shown
		var shownFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var folderShowOrder = new List<string>();

		for (int i = 0; i < referenceLines.Count; i++)
		{
			var referenceLine = referenceLines[i];
			var fileName = ExtractFileNameFromReferenceLine(referenceLine);
			var file = allFiles.FirstOrDefault(f => f.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));

			if (file?.Directory != null)
			{
				var relativePath = GetRelativePath(baseDirectory.FullName, file.Directory.FullName);
				var isRootFile = fileName.Equals(rootFile.Name, StringComparison.OrdinalIgnoreCase); if (isRootFile)
				{
					// Root file - show folder icon with folder name
					var folderName = file.Directory.Name;
					var hasChildItems = hasRootFiles || uniqueFolders.Count != 0;
					lines.Add($" 📁 {folderName} /");
				}
				else if (string.IsNullOrEmpty(relativePath) || relativePath == ".")
				{
					// File in root directory - show continuation line from root
					lines.Add(" │");
				}
				else
				{
					// File in subdirectory
					if (!shownFolders.Contains(relativePath))
					{
						// First file from this folder - show the folder
						var cleanFolderName = relativePath.TrimEnd('\\', '/');
						folderShowOrder.Add(relativePath);
						var isLastFolder = folderShowOrder.Count == totalFoldersAtRoot;
						var connector = isLastFolder ? "└── " : "├── ";
						lines.Add($" {connector}📁 {cleanFolderName}");
						shownFolders.Add(relativePath);
					}
					else
					{
						// Subsequent files from same folder
						// If the current folder was shown with └── (last folder), use spaces; otherwise use continuation (│)
						var currentFolderIndex = folderShowOrder.IndexOf(relativePath) + 1; // +1 because it was added when first shown
						var wasLastFolder = currentFolderIndex == totalFoldersAtRoot;
						lines.Add(wasLastFolder ? "                " : " │");
					}
				}
			}
			else
			{
				lines.Add("");
			}
		}

		return lines;
	}

	/// <summary>
	/// Builds a hierarchy of folders to determine tree structure
	/// </summary>
	private static Dictionary<string, FolderNode> BuildFolderHierarchy(List<FileInfo> allFiles, DirectoryInfo baseDirectory)
	{
		var nodes = new Dictionary<string, FolderNode>(StringComparer.OrdinalIgnoreCase);
		var root = new FolderNode { Path = baseDirectory.FullName, Name = "", IsRoot = true };
		nodes[baseDirectory.FullName] = root;

		foreach (var file in allFiles)
		{
			if (file.Directory == null) continue;

			var dirPath = file.Directory.FullName;
			if (!nodes.ContainsKey(dirPath))
			{
				var relativePath = GetRelativePath(baseDirectory.FullName, dirPath);
				var pathParts = relativePath.Split([Path.DirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);

				var currentPath = baseDirectory.FullName;
				FolderNode parentNode = root;

				foreach (var part in pathParts)
				{
					currentPath = Path.Combine(currentPath, part);

					if (!nodes.TryGetValue(currentPath, out FolderNode? value))
					{
						var node = new FolderNode
						{
							Path = currentPath,
							Name = part,
							Parent = parentNode
						};
						value = node;
						nodes[currentPath] = value;
						parentNode.Children.Add(node);
					}

					parentNode = value;
				}
			}
		}

		return nodes;
	}

	/// <summary>
	/// Gets the folder tree line for a specific file
	/// </summary>
	private static string GetFolderTreeLine(FileInfo file, Dictionary<string, FolderNode> folderHierarchy, bool isRootFile)
	{
		if (isRootFile)
		{
			return "📄"; // Just the file icon for the root file
		}

		var dirPath = file.Directory?.FullName;
		if (dirPath != null && folderHierarchy.TryGetValue(dirPath, out var node))
		{
			// Build the tree path for this directory
			var pathParts = new List<string>();
			var current = node;

			while (current != null && !current.IsRoot)
			{
				pathParts.Insert(0, current.Name);
				current = current.Parent;
			}

			if (pathParts.Count == 0)
			{
				return "│"; // File in root directory, just continuation line
			}
		// Build tree structure for this path using StringBuilder to avoid allocations
		var sb = new System.Text.StringBuilder(pathParts.Count * 20); // Pre-size estimate
		for (int i = pathParts.Count - 1; i >= 0; i--)
		{
			if (i == 0)
			{
				sb.Append("├── 📁 ").Append(pathParts[i]).Append('/');
			}
			else
			{
				sb.Insert(0, "│   ");
			}
		}

		return sb.ToString();
		}

		return "";
	}

	/// <summary>
	/// Node representing a folder in the hierarchy
	/// </summary>
	private class FolderNode
	{
		public string Path { get; set; } = "";
		public string Name { get; set; } = "";
		public bool IsRoot { get; set; }
		public FolderNode? Parent { get; set; }
		public List<FolderNode> Children { get; set; } = [];
	}

	/// <summary>
	/// Extracts the file name from a reference tree line by removing tree formatting
	/// </summary>
	private static string ExtractFileNameFromReferenceLine(string referenceLine)
	{
		// Remove tree characters and trim
		var cleaned = referenceLine.Replace("├── ", "").Replace("└── ", "").Replace("│   ", "").Replace("    ", "").Trim();
		return cleaned;
	}

	/// <summary>
	/// Builds folder structure lines showing directory hierarchy
	/// </summary>
	private static List<string> BuildFolderStructureLines(List<FileInfo> allFiles, DirectoryInfo baseDirectory)
	{
		var lines = new List<string>();

		// Group files by directory
		var filesByDirectory = allFiles.GroupBy(f => f.Directory?.FullName ?? "")
			.Where(g => !string.IsNullOrEmpty(g.Key))
			.OrderBy(g => g.Key)
			.ToList();

		foreach (var directoryGroup in filesByDirectory)
		{
			var dirPath = directoryGroup.Key;

			// Get relative path for display
			var relativePath = GetRelativePath(baseDirectory.FullName, dirPath);
			var displayPath = string.IsNullOrEmpty(relativePath) ? "." : relativePath;

			lines.Add($"📁 {displayPath}");

			// Add files in this directory with indentation
			var filesInDir = directoryGroup.OrderBy(f => f.Name).ToList();
			for (int i = 0; i < filesInDir.Count; i++)
			{
				var isLast = i == filesInDir.Count - 1;
				var prefix = isLast ? "└── " : "├── ";
				lines.Add($"  {prefix}{filesInDir[i].Name}");
			}

			// Add spacing between directories
			if (directoryGroup != filesByDirectory.Last())
				lines.Add("");
		}

		return lines;
	}

	/// <summary>
	/// Builds reference tree lines showing include relationships
	/// </summary>
	private static List<string> BuildReferenceTreeLines(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		var lines = new List<string>();
		var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootFile.FullName };

		lines.Add(rootFile.Name);

		if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
		{
			var validChildren = children.Where(path =>
				!string.IsNullOrEmpty(path) &&
				!string.Equals(path, rootFile.FullName, StringComparison.OrdinalIgnoreCase)).ToList();

			BuildReferenceTreeLinesRecursive(validChildren, fileRelationships, lines, visitedPaths, "");
		}

		return lines;
	}

	/// <summary>
	/// Recursively builds reference tree lines with proper tree formatting
	/// </summary>
	private static void BuildReferenceTreeLinesRecursive(List<string> childPaths,
		Dictionary<string, List<string>> fileRelationships,
		List<string> lines,
		HashSet<string> visitedPaths,
		string indent)
	{
		for (int i = 0; i < childPaths.Count; i++)
		{
			var childPath = childPaths[i];
			var isLast = i == childPaths.Count - 1;

			if (visitedPaths.Contains(childPath))
				continue;

			visitedPaths.Add(childPath);

			var childFile = new FileInfo(childPath);
			var prefix = isLast ? "└── " : "├── ";
			lines.Add($"{indent}{prefix}{childFile.Name}");

			// Recursively add grandchildren
			if (fileRelationships.TryGetValue(childPath, out var grandChildren))
			{
				var validGrandChildren = grandChildren.Where(path =>
					!string.IsNullOrEmpty(path) &&
					!string.Equals(path, childPath, StringComparison.OrdinalIgnoreCase)).ToList();

				if (validGrandChildren.Count != 0)
				{
					var newIndent = indent + (isLast ? "    " : "│   ");
					BuildReferenceTreeLinesRecursive(validGrandChildren, fileRelationships, lines, visitedPaths, newIndent);
				}
			}
		}
	}

	/// <summary>
	/// Gets relative path from root to target directory, always using forward slashes
	/// </summary>
	private static string GetRelativePath(string rootPath, string targetPath)
	{
		try
		{
			var rootUri = new Uri(rootPath + Path.DirectorySeparatorChar);
			var targetUri = new Uri(targetPath + Path.DirectorySeparatorChar);
			var relativeUri = rootUri.MakeRelativeUri(targetUri);
			// Always use forward slashes for consistency, regardless of platform
			return Uri.UnescapeDataString(relativeUri.ToString());
		}
		catch
		{
			// Fallback: convert backslashes to forward slashes for consistency
			return targetPath.Replace('\\', '/');
		}
	}
}
