using Spectre.Console;

namespace MergeIncludes;

public sealed partial class CombineCommand
{
    /// <summary>
    /// Helper method to add children to a folder grouped tree
    /// </summary>
    private void AddChildrenToFolderGroupedTree(
        TreeNode parentNode,
        string parentPath,
        Dictionary<string, List<string>> fileRelationships,
        DirectoryInfo workspaceRoot,
        Dictionary<string, int> fileIds,
        HashSet<string> repeatedFiles,
        Dictionary<string, TreeNode> directoryNodesMap,
        string idFormat)
    {
        if (!fileRelationships.TryGetValue(parentPath, out var children))
        {
            return;
        }

        // Group files by directory for folder headers
        var filesByDirectory = new Dictionary<string, List<FileInfo>>();
        
        foreach (var childPath in children)
        {
            var fileInfo = new FileInfo(childPath);
            var dirPath = fileInfo.DirectoryName ?? string.Empty;
            
            if (!filesByDirectory.ContainsKey(dirPath))
            {
                filesByDirectory[dirPath] = new List<FileInfo>();
            }
            
            filesByDirectory[dirPath].Add(fileInfo);
        }
        
        // Process each directory group
        foreach (var dirEntry in filesByDirectory)
        {
            var dirPath = dirEntry.Key;
            var dirFiles = dirEntry.Value;
            
            // Skip parent directory as we don't need folder headers for it
            if (string.Equals(dirPath, Path.GetDirectoryName(parentPath), StringComparison.OrdinalIgnoreCase))
            {
                // Files in the same directory as parent - just add them directly
                foreach (var fileInfo in dirFiles)
                {
                    var fileName = fileInfo.Name;
                    bool isRepeated = repeatedFiles.Contains(fileInfo.FullName);
                    
                    string displayText = isRepeated && fileIds.TryGetValue(fileInfo.FullName, out int fileId) ?
                        $"{fileName} {string.Format(idFormat, fileId)}" : fileName;
                        
                    var style = isRepeated ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
                    var fileNode = parentNode.AddNode(new Text(displayText, style));
                    
                    // Recursively add children
                    if (fileRelationships.ContainsKey(fileInfo.FullName))
                    {
                        AddChildrenToFolderGroupedTree(fileNode, fileInfo.FullName, fileRelationships,
                            workspaceRoot, fileIds, repeatedFiles, directoryNodesMap, idFormat);
                    }
                }
            }
            else
            {
                // Create or reuse a folder header for this directory
                TreeNode folderNode;
                
                if (directoryNodesMap.TryGetValue(dirPath, out var existingNode))
                {
                    // Use existing folder node
                    folderNode = existingNode;
                }
                else
                {
                    // Create a new folder header
                    var dirInfo = new DirectoryInfo(dirPath);
                    var folderName = GetProjectRelativePath(dirInfo, workspaceRoot);
                    
                    // Use [DIR] instead of emoji for better console compatibility
                    var folderText = new Text($"[DIR] {folderName}", new Style(Color.Yellow3));
                    folderNode = parentNode.AddNode(folderText);
                    directoryNodesMap[dirPath] = folderNode;
                }
                
                // Add files under the folder header
                foreach (var fileInfo in dirFiles)
                {
                    var fileName = fileInfo.Name;
                    bool isRepeated = repeatedFiles.Contains(fileInfo.FullName);
                    
                    string displayText = isRepeated && fileIds.TryGetValue(fileInfo.FullName, out int fileId) ?
                        $"{fileName} {string.Format(idFormat, fileId)}" : fileName;
                        
                    var style = isRepeated ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
                    var fileNode = folderNode.AddNode(new Text(displayText, style));
                    
                    // Recursively add children
                    if (fileRelationships.ContainsKey(fileInfo.FullName))
                    {
                        AddChildrenToFolderGroupedTree(fileNode, fileInfo.FullName, fileRelationships,
                            workspaceRoot, fileIds, repeatedFiles, directoryNodesMap, idFormat);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds children to a simple tree node format (just names and IDs)
    /// </summary>
    private void AddChildrenToRepeatsOnlyTree(
        TreeNode parentNode, 
        string parentPath, 
        Dictionary<string, List<string>> fileRelationships, 
        Dictionary<string, int> fileIds, 
        HashSet<string> repeatedFiles,
        string idFormat)
    {
        if (!fileRelationships.TryGetValue(parentPath, out var children))
        {
            return;
        }

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
            
            var childNode = parentNode.AddNode(new Text(displayText, style));
            
            if (fileRelationships.ContainsKey(childPath))
            {
                AddChildrenToRepeatsOnlyTree(childNode, childPath, fileRelationships, fileIds, repeatedFiles, idFormat);
            }
        }
    }

    /// <summary>
    /// Adds children to a simple tree node format (just names and IDs)
    /// </summary>
	private void AddChildrenToSimpleTree(
	    TreeNode parentNode, 
	    string parentPath, 
	    Dictionary<string, List<string>> fileRelationships, 
	    Dictionary<string, int> fileIds, 
	    ref int nextId,
	    string idFormat)
	{
		if (!fileRelationships.TryGetValue(parentPath, out var children))
		{
			return;
		}

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
			
			var childNode = parentNode.AddNode(new Text(displayText, style));
			
			if (fileRelationships.ContainsKey(childPath))
            {
			    AddChildrenToSimpleTree(childNode, childPath, fileRelationships, fileIds, ref nextId, idFormat);
            }
		}
	}

    /// <summary>
    /// Adds children to a folder tree node format (names with folder info)
    /// </summary>
	private void AddChildrenToFolderTreeWithText(
	    TreeNode parentNode, 
	    string parentPath, 
	    Dictionary<string, List<string>> fileRelationships, 
	    DirectoryInfo workspaceRoot, 
	    Dictionary<string, int> fileIds,
	    ref int nextId,
	    string idFormat)
	{
		if (!fileRelationships.TryGetValue(parentPath, out var children))
		{
			return;
		}

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
			
			var childNode = parentNode.AddNode(new Text(displayText, style));
			
			if (fileRelationships.ContainsKey(childPath))
            {
			    AddChildrenToFolderTreeWithText(childNode, childPath, fileRelationships, workspaceRoot, fileIds, ref nextId, idFormat);
            }
		}
	}
	
    /// <summary>
    /// Adds children to a full path tree node format
    /// </summary>
	private void AddChildrenToRelativePathTreeWithText(
	    TreeNode parentNode, 
	    string parentPath, 
	    Dictionary<string, List<string>> fileRelationships, 
	    string baseDirectoryPath, 
	    Dictionary<string, int> fileIds,
	    ref int nextId,
	    string idFormat)
	{
		if (!fileRelationships.TryGetValue(parentPath, out var children))
		{
			return;
		}

		foreach (var childPath in children)
		{
			// Get a path relative to the root file directory
			var relativePath = GetRelativeFilePath(childPath, baseDirectoryPath);
			
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
			var childNode = parentNode.AddNode(text);
			
			if (fileRelationships.ContainsKey(childPath))
            {
			    AddChildrenToRelativePathTreeWithText(childNode, childPath, fileRelationships, baseDirectoryPath, fileIds, ref nextId, idFormat);
            }
		}
	}
	
	/// <summary>
	/// Calculates the total number of unique files in the tree
	/// </summary>
	private int CountAllFilesInTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
	    var uniqueFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	    uniqueFiles.Add(rootFile.FullName);
	    
	    // Helper local function to count files recursively
	    void CountFilesRecursive(string currentFile)
	    {
	        if (!fileRelationships.TryGetValue(currentFile, out var children))
	            return;
	            
	        foreach (var child in children)
	        {
	            uniqueFiles.Add(child);
	            CountFilesRecursive(child);
	        }
	    }
	    
	    CountFilesRecursive(rootFile.FullName);
	    return uniqueFiles.Count;
	}
	
    /// <summary>
    /// Finds all files that are referenced multiple times in the tree
    /// </summary>
    private HashSet<string> FindRepeatedFiles(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        var fileOccurrences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var repeatedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Add the root file
        fileOccurrences[rootFile.FullName] = 1;
        
        // Helper function to count occurrences
        void CountOccurrencesRecursive(string currentFile)
        {
            if (!fileRelationships.TryGetValue(currentFile, out var children))
                return;
                
            foreach (var child in children)
            {
                // Increment the counter for this file
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
                
                CountOccurrencesRecursive(child);
            }
        }
        
        CountOccurrencesRecursive(rootFile.FullName);
        return repeatedFiles;
    }
	
	/// <summary>
	/// Determines if a file is referenced multiple times in the file tree
	/// </summary>
	private bool IsFileRepeated(string filePath, Dictionary<string, List<string>> fileRelationships)
	{
	    int count = 0;
	    
	    // Helper local function to count occurrences
	    void CountOccurrences(string currentFile, string targetFile)
	    {
	        if (!fileRelationships.TryGetValue(currentFile, out var children))
	            return;
	            
	        foreach (var child in children)
	        {
	            if (string.Equals(child, targetFile, StringComparison.OrdinalIgnoreCase))
	                count++;
	                
	            CountOccurrences(child, targetFile);
	        }
	    }
	    
	    // Count occurrences in the entire tree
	    foreach (var entry in fileRelationships)
	    {
	        foreach (var child in entry.Value)
	        {
	            if (string.Equals(child, filePath, StringComparison.OrdinalIgnoreCase))
	                count++;
	        }
	    }
	    
	    return count > 1;
	}
	
	/// <summary>
	/// Gets the appropriate format string for IDs based on the total number of files
	/// </summary>
	private string GetIdFormat(int totalFiles)
	{
	    if (totalFiles < 10)
	        return "[{0}]";
	    else if (totalFiles < 100)
	        return "[{0:D2}]";
	    else if (totalFiles < 1000)
	        return "[{0:D3}]";
	    else
	        return "[{0:D4}]";
	}
}