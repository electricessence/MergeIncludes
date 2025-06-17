using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;
using System.Text;
using MergeIncludes.TreeBuilders;

namespace MergeIncludes.Renderables;

/// <summary>
/// Renders the side-by-side folder structure and reference trees view using Tree widgets
/// </summary>
public sealed class StructureAndReferenceView : IRenderable
{
	private readonly IRenderable _content;

	/// <summary>
	/// Initializes a new instance of the StructureAndReferenceView class
	/// </summary>
	/// <param name="rootFile">The root file being processed</param>
	/// <param name="fileRelationships">Dictionary mapping files to their dependencies</param>
	public StructureAndReferenceView(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		if (rootFile == null) throw new ArgumentNullException(nameof(rootFile));
		if (fileRelationships == null) throw new ArgumentNullException(nameof(fileRelationships));

		_content = CreateTwoColumnTreeTable(rootFile, fileRelationships);
	}

	/// <inheritdoc/>
	public Measurement Measure(RenderOptions options, int maxWidth)
	{
		// Delegate to the internal content for measurement
		return _content.Measure(options, maxWidth);
	}

	/// <inheritdoc/>
	public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
	{
		// Delegate to the internal content for rendering
		return _content.Render(options, maxWidth);
	}	private static IRenderable CreateTwoColumnTreeTable(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Create the left column: folder structure tree using aligned builder
		var folderTree = AlignedFolderTreeBuilder.FromDependencies(rootFile, fileRelationships);

		// Create the middle separator column with just "/"
		var separator = new Text("/", new Style(Color.Grey));

		// Create the right column: reference tree
		var referenceTree = CreateReferenceTree(rootFile, fileRelationships);

		// Create a table with three columns for side-by-side display
		var table = new Table()
			.Border(TableBorder.None)
			.HideHeaders()
			.Collapse()  // Makes table take up minimal width
			.AddColumn(new TableColumn("Folder") { NoWrap = true, Padding = new Padding(0, 0, 1, 0) })  // Small right padding
			.AddColumn(new TableColumn("Separator") { NoWrap = true, Width = 1, Padding = new Padding(0, 0, 1, 0) })  // Small padding on both sides
			.AddColumn(new TableColumn("References") { NoWrap = true, Padding = new Padding(0, 0, 0, 0) });

		// Add the trees with separator
		table.AddRow(folderTree, separator, referenceTree);

		return table;
	}	private static IRenderable CreateFolderStructureTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Create root folder - blue color, no bold
		var rootFolderName = $"📁 {rootFile.Directory!.Name}";
		var rootStyle = new Style(foreground: Color.Blue);
		
		var rootText = HyperLink.For(rootFile.Directory.FullName, rootFolderName, rootStyle);
		var tree = new TreeMinimalWidth(rootText);

		// Track which files we've seen and assign them unique IDs (same as reference tree)
		var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		var nextId = 0;
		
		// Assign ID to root file so if it appears in dependencies it's treated as repeat
		fileIds[rootFile.FullName] = nextId++;
		
		// Track visited paths to prevent infinite recursion due to circular references
		var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		// Use smart folder context to track changes and avoid duplication
		var folderContext = new SmartFolderContext(rootFile.Directory);

		// Build folder alignment using the SAME traversal logic as the reference tree
		BuildSmartFolderAlignmentRecursive(tree, rootFile.FullName, fileRelationships, fileIds, ref nextId, visitedPaths, folderContext);
		return tree;
	}

	private static IRenderable CreateReferenceTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Create root node with yellow color (not bold)
		// HyperLink.For handles Windows Terminal detection internally
		var rootText = HyperLink.For(rootFile, new Style(Color.Yellow));
		var tree = new TreeMinimalWidth(rootText);
		// Track which files we've seen and assign them unique IDs
		var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		var nextId = 0;

		// Assign ID to root file so if it appears in dependencies it's treated as repeat
		fileIds[rootFile.FullName] = nextId++;
		// Track visited paths to prevent infinite recursion due to circular references
		var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		BuildCompleteReferenceTreeRecursive(tree, rootFile.FullName, fileRelationships, fileIds, ref nextId, visitedPaths);

		return tree;
	}/// <summary>
	/// Recursively builds the complete reference tree hierarchy with ALL references and consistent IDs
	/// </summary>
	private static void BuildCompleteReferenceTreeRecursive(
		IHasTreeNodes tree, string filePath,
		Dictionary<string, List<string>> fileRelationships,
		Dictionary<string, int> fileIds,
		ref int nextId,
		HashSet<string> visitedPaths)
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

				// Check for circular references - if we've visited this path in the current branch
				bool isCircularReference = visitedPaths.Contains(dependency);

				// Always assign an ID and display the file
				if (isFirstOccurrence)
				{
					// First occurrence: assign new ID
					fileIds[dependency] = nextId++;				}

				var fileId = fileIds[dependency];
				var fileName = dependencyFile.Name;
				var displayText = $"{fileName} [{fileId}]"; if (isCircularReference)
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
					var childNode = tree.AddNode(dependencyText);					// Recursively build dependencies for ALL occurrences
					BuildCompleteReferenceTreeNodeRecursive(
						childNode,
						dependency,
						fileRelationships,
						fileIds,
						ref nextId,
						new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
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
						new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
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
		HashSet<string> visitedPaths)
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
				continue; if (File.Exists(dependency))
			{
				var dependencyFile = new FileInfo(dependency);

				// Check if this is the first time we've seen this file
				bool isFirstOccurrence = !fileIds.ContainsKey(dependency);

				// Check for circular references - if we've visited this path in the current branch
				bool isCircularReference = visitedPaths.Contains(dependency);

				// Always assign an ID and display the file
				if (isFirstOccurrence)
				{
					// First occurrence: assign new ID
					fileIds[dependency] = nextId++;
				}

				var fileId = fileIds[dependency];
				var fileName = dependencyFile.Name;
				var displayText = $"{fileName} [{fileId}]";
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

					// Continue recursively for ALL occurrences
					BuildCompleteReferenceTreeNodeRecursive(
						childNode,
						dependency,
						fileRelationships,
						fileIds,
						ref nextId,
						new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
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
						new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
				}
			}
		}

		// Remove from visited before returning to allow the file to appear in other branches
		visitedPaths.Remove(filePath);
	}	/// <summary>
	/// Context tracker for smart folder display to avoid duplication and show proper hierarchy
	/// </summary>
	private class SmartFolderContext
	{
		public DirectoryInfo RootDirectory { get; }
		public string CurrentFolderPath { get; set; } = "";
		public int CurrentDepth { get; set; } = 0;
		public List<string> DisplayedFolders { get; } = new();

		public SmartFolderContext(DirectoryInfo rootDirectory)
		{
			RootDirectory = rootDirectory;
		}
	}

	/// <summary>
	/// Build smart folder alignment that mirrors the reference tree traversal exactly
	/// </summary>
	private static void BuildSmartFolderAlignmentRecursive(
		TreeMinimalWidth tree, 
		string filePath,
		Dictionary<string, List<string>> fileRelationships, 
		Dictionary<string, int> fileIds, 
		ref int nextId, 
		HashSet<string> visitedPaths,
		SmartFolderContext context)
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

				// Check for circular references - if we've visited this path in the current branch
				bool isCircularReference = visitedPaths.Contains(dependency);

				// Always assign an ID and display the file
				if (isFirstOccurrence)
				{
					// First occurrence: assign new ID
					fileIds[dependency] = nextId++;
				}

				// Calculate relative path for this dependency
				var relativePath = Path.GetRelativePath(context.RootDirectory.FullName, dependencyFile.Directory!.FullName);
				relativePath = relativePath.Replace('\\', '/');
				if (relativePath == ".") relativePath = "";

				// Create smart folder line that tracks context
				var folderLine = CreateSmartFolderLine(relativePath, context);
				tree.AddNode(folderLine);

				if (!isCircularReference)
				{
					// Continue recursively
					BuildSmartFolderAlignmentRecursive(
						tree,
						dependency,
						fileRelationships,
						fileIds,
						ref nextId,
						new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase),
						context);
				}
			}
		}

		// Remove from visited before returning to allow the file to appear in other branches
		visitedPaths.Remove(filePath);
	}
	/// <summary>
	/// Creates smart folder line that only shows folder names when path changes
	/// Shows proper tree hierarchy and avoids duplication
	/// </summary>
	private static IRenderable CreateSmartFolderLine(string relativePath, SmartFolderContext context)
	{
		if (relativePath == context.CurrentFolderPath)
		{
			// Same folder as previous line - show continuation indicator
			var continuation = GetFolderContinuation(context.CurrentDepth);
			return new Text(continuation, new Style(Color.DarkGreen));
		}
		else
		{
			// Different folder - update context and determine what to show
			var previousPath = context.CurrentFolderPath;
			context.CurrentFolderPath = relativePath;
			
			if (string.IsNullOrEmpty(relativePath))
			{
				// Root folder file
				context.CurrentDepth = 0;
				return new Text("│", new Style(Color.DarkGreen));
			}
			else
			{
				// Subfolder - determine what needs to be shown
				var pathParts = relativePath.Split('/');
				context.CurrentDepth = pathParts.Length;
				
				// Determine if this is a new folder branch or continuation
				var folderDisplay = DetermineSmartFolderDisplay(previousPath, relativePath, pathParts, context);
				return new Text(folderDisplay, new Style(Color.Green));
			}
		}
	}

	/// <summary>
	/// Determines smart folder display based on path changes and context
	/// </summary>
	private static string DetermineSmartFolderDisplay(string previousPath, string currentPath, string[] pathParts, SmartFolderContext context)
	{
		// For now, implement a simple but effective approach:
		// Show the last folder name with proper indentation based on depth
		
		var depth = pathParts.Length;
		var lastFolder = pathParts.Last();
		
		// Check if we've shown this folder path before
		var fullPathKey = string.Join("/", pathParts);
		
		if (context.DisplayedFolders.Contains(fullPathKey))
		{
			// This exact folder path was already shown - use continuation
			var indent = depth > 1 ? new string(' ', (depth - 1) * 2) : "";
			return $"{indent}│";
		}
		else
		{
			// New folder path - show it and mark as displayed
			context.DisplayedFolders.Add(fullPathKey);
			var indent = depth > 1 ? new string(' ', (depth - 1) * 2) : "";
			var connector = "├── ";
			return $"{indent}{connector}📁 {lastFolder}/";
		}
	}
	/// <summary>
	/// Gets continuation indicator for files in the same folder
	/// </summary>
	private static string GetFolderContinuation(int depth)
	{
		if (depth == 0)
		{
			return "│"; // Root folder continuation
		}
		else
		{
			var indent = new string(' ', (depth - 1) * 2);
			return $"{indent}│";
		}
	}

	/// <summary>
	/// Get the display name for a folder relative to the base directory
	/// </summary>
	private static string GetFolderDisplayName(DirectoryInfo baseDirectory, DirectoryInfo targetDirectory)
	{
		if (targetDirectory.FullName.StartsWith(baseDirectory.FullName, StringComparison.OrdinalIgnoreCase))
		{
			// Subdirectory - show relative path
			var relativePath = Path.GetRelativePath(baseDirectory.FullName, targetDirectory.FullName);
			return $"📁 {relativePath.Replace('\\', '/')}/";
		}
		else
		{
			// Outside base directory - show relative path
			var relativePath = Path.GetRelativePath(baseDirectory.FullName, targetDirectory.FullName);
			return $"📁 {relativePath.Replace('\\', '/')}/";
		}
	}
}
