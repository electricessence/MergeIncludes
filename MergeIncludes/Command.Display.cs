using Spectre.Console;

namespace MergeIncludes;

public sealed partial class CombineCommand
{
    /// <summary>
    /// Displays the tree with simple filenames and IDs for all files
    /// </summary>
    private void DisplaySimpleFileTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Create a tree with the root file as the root node
		var rootName = Path.GetFileName(rootFile.FullName);
        
        // Create file ID map and assign the root file ID 0
        var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var nextId = 0;
        fileIds[rootFile.FullName] = nextId++;
        
        // Format ID with the appropriate number of digits based on total files
        int totalFiles = CountAllFilesInTree(rootFile, fileRelationships);
        string idFormat = GetIdFormat(totalFiles);
        
        var rootText = new Text($"{rootName} {string.Format(idFormat, fileIds[rootFile.FullName])}", Color.LightSkyBlue1);
		var tree = new Tree(rootText);
		tree.Guide = TreeGuide.Line;

		// Recursively build the tree
		if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
		{
			foreach (var childPath in children)
			{
				var fileName = Path.GetFileName(childPath);
				
				// Check if this file has been seen before
				if (!fileIds.TryGetValue(childPath, out int fileId))
				{
				    // If not, assign it a new ID
				    fileId = nextId++;
				    fileIds[childPath] = fileId;
				}
				
				// Display the ID for this file
				string displayText = $"{fileName} {string.Format(idFormat, fileId)}";
				
				// Use yellow for repeats - those that would be encountered again
				bool isRepeat = IsFileRepeated(childPath, fileRelationships);
				var style = isRepeat ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
				
				var childNode = tree.AddNode(new Text(displayText, style));
				
				// Continue building the tree
				if (fileRelationships.ContainsKey(childPath))
                {
				    AddChildrenToSimpleTree(childNode, childPath, fileRelationships, fileIds, ref nextId, idFormat);
                }
			}
		}

		// Write the tree to the console - match the exact padding from the test files
		var panel = new Panel(tree)
		{
			Header = new PanelHeader("[white]Files included in merge:[/]"),
			Expand = true,
			Border = BoxBorder.Rounded,
			Padding = new Padding(1, 0, 1, 0)
		};
		_console.Write(panel);
	}

    /// <summary>
    /// Displays the tree showing only IDs for files that are repeated
    /// </summary>
    private void DisplayRepeatsOnlyFileTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        // Create a tree with the root file as the root node
        var rootName = Path.GetFileName(rootFile.FullName);
        
        // First pass: identify all repeated files
        var repeatedFiles = FindRepeatedFiles(rootFile, fileRelationships);
        
        // Create file ID map - only assign IDs to files that are repeated
        var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var nextId = 0;
        
        // Assign IDs to repeated files first
        foreach (var repeatedFile in repeatedFiles)
        {
            fileIds[repeatedFile] = nextId++;
        }
        
        // Format ID with the appropriate number of digits based on number of repeated files
        string idFormat = GetIdFormat(repeatedFiles.Count > 0 ? repeatedFiles.Count : 1);
        
        // Don't show ID for root file unless it's repeated elsewhere in the tree
        string rootDisplay = repeatedFiles.Contains(rootFile.FullName) ? 
            $"{rootName} {string.Format(idFormat, fileIds[rootFile.FullName])}" : rootName;
        
        var rootText = new Text(rootDisplay, Color.LightSkyBlue1);
        var tree = new Tree(rootText);
        tree.Guide = TreeGuide.Line;

        // Recursively build the tree
        if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
        {
            foreach (var childPath in children)
            {
                var fileName = Path.GetFileName(childPath);
                
                // Check if this file is repeated
                bool isRepeated = repeatedFiles.Contains(childPath);
                
                // Display text - only include ID if the file is repeated
                string displayText = isRepeated && fileIds.TryGetValue(childPath, out int fileId) ?
                    $"{fileName} {string.Format(idFormat, fileId)}" : fileName;
                
                // Use yellow for repeated files
                var style = isRepeated ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
                
                var childNode = tree.AddNode(new Text(displayText, style));
                
                // Continue building the tree
                if (fileRelationships.ContainsKey(childPath))
                {
                    AddChildrenToRepeatsOnlyTree(childNode, childPath, fileRelationships, fileIds, repeatedFiles, idFormat);
                }
            }
        }

        // Write the tree to the console - match the exact padding from the test files
        var panel = new Panel(tree)
        {
            Header = new PanelHeader("[white]Files included in merge:[/]"),
            Expand = true,
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0, 1, 0)
        };
        _console.Write(panel);
    }

    /// <summary>
    /// Displays the tree with folder structure information
    /// </summary>
	private void DisplayFolderStructureTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Get the base directory of the root file to calculate relative paths
		var baseDirectory = rootFile.Directory ?? throw new InvalidOperationException("Root file directory cannot be null");
		
		 // Get the workspace root directory (try to detect project structure)
        var workspaceRoot = GetWorkspaceRoot(baseDirectory);
		
		// Create a tree with the root file as the root node
		var rootName = rootFile.Name;
		var projectFolder = GetProjectRelativePath(rootFile.Directory, workspaceRoot);
		
        // Create file ID map and assign the root file ID 0
        var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var nextId = 0;
        fileIds[rootFile.FullName] = nextId++;
        
        // Format ID with the appropriate number of digits based on total files
        int totalFiles = CountAllFilesInTree(rootFile, fileRelationships);
        string idFormat = GetIdFormat(totalFiles);
		
		// Create a styled text node for the root
		var rootText = new Text($"{rootName} {string.Format(idFormat, fileIds[rootFile.FullName])} (in {projectFolder})", Color.LightSkyBlue1);
		var tree = new Tree(rootText);
		tree.Guide = TreeGuide.Line;

		// Recursively build the tree
		if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
		{
			foreach (var childPath in children)
			{
				var fileInfo = new FileInfo(childPath);
				var folderName = GetProjectRelativePath(fileInfo.Directory, workspaceRoot);
				
				// Check if this file has been seen before
				if (!fileIds.TryGetValue(childPath, out int fileId))
				{
				    // If not, assign it a new ID
				    fileId = nextId++;
				    fileIds[childPath] = fileId;
				}
				
				// Display the ID for this file along with folder info
				string displayText = $"{fileInfo.Name} {string.Format(idFormat, fileId)} (in {folderName})";
				
				// Use yellow for repeats - those that would be encountered again
				bool isRepeat = IsFileRepeated(childPath, fileRelationships);
				var style = isRepeat ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
				
				var childNode = tree.AddNode(new Text(displayText, style));
				
				if (fileRelationships.ContainsKey(childPath))
                {
				    AddChildrenToFolderTreeWithText(childNode, childPath, fileRelationships, workspaceRoot, fileIds, ref nextId, idFormat);
                }
			}
		}

		// Write the tree to the console - match the exact padding from the test files
		var panel = new Panel(tree)
		{
			Header = new PanelHeader("[white]Files included in merge with folder structure:[/]"),
			Expand = true,
			Border = BoxBorder.Rounded,
			Padding = new Padding(1, 0, 1, 0)
		};
		_console.Write(panel);
	}

    /// <summary>
    /// Displays the tree with full path information for each file
    /// </summary>
	private void DisplayFullPathsTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Get the base directory of the root file to calculate relative paths
		var baseDirectory = rootFile.Directory ?? throw new InvalidOperationException("Root file directory cannot be null");
		
        // Create file ID map and assign the root file ID 0
        var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var nextId = 0;
        fileIds[rootFile.FullName] = nextId++;
        
        // Format ID with the appropriate number of digits based on total files
        int totalFiles = CountAllFilesInTree(rootFile, fileRelationships);
        string idFormat = GetIdFormat(totalFiles);
        
		// Create a tree with the root file as the root node - using just the name for the root
		var rootText = new Text($"{rootFile.Name} {string.Format(idFormat, fileIds[rootFile.FullName])}", Color.LightSkyBlue1);
		var tree = new Tree(rootText);
		tree.Guide = TreeGuide.Line;

		// Recursively build the tree
		if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
		{
			foreach (var childPath in children)
			{
				var childFile = new FileInfo(childPath);
				// Get a path relative to the root file directory
				var relativePath = GetRelativeFilePath(childFile.FullName, baseDirectory.FullName);
				
				// Check if this file has been seen before
				if (!fileIds.TryGetValue(childPath, out int fileId))
				{
				    // If not, assign it a new ID
				    fileId = nextId++;
				    fileIds[childPath] = fileId;
				}
				
				// Add the ID to the display text
				string displayText;
				if (relativePath.Contains(" (in "))
				{
				    var parts = relativePath.Split(new[] { " (in " }, 2, StringSplitOptions.None);
				    displayText = $"{parts[0]} {string.Format(idFormat, fileId)} (in {parts[1]}";
				}
				else
				{
				    displayText = $"{relativePath} {string.Format(idFormat, fileId)}";
				}
				
				// Use yellow for repeats - those that would be encountered again
				bool isRepeat = IsFileRepeated(childPath, fileRelationships);
				var style = isRepeat ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
				
				var text = new Text(displayText, style);
				var childNode = tree.AddNode(text);
				
				if (fileRelationships.ContainsKey(childPath))
                {
				    AddChildrenToRelativePathTreeWithText(childNode, childPath, fileRelationships, baseDirectory.FullName, fileIds, ref nextId, idFormat);
                }
			}
		}

		// Write the tree to the console - match the exact padding from the test files
		var panel = new Panel(tree)
		{
			Header = new PanelHeader("[white]Files included in merge with paths:[/]"),
			Expand = true,
			Border = BoxBorder.Rounded,
			Padding = new Padding(1, 0, 1, 0)
		};
		_console.Write(panel);
	}
}