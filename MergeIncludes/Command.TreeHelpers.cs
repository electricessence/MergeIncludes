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
        string idFormat,
        HashSet<string>? visitedPaths = null)
    {
        // Initialize or use existing visited paths set
        visitedPaths ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Skip if we've already visited this path in the current branch (cycle detection)
        if (!visitedPaths.Add(parentPath))
        {
            // If this is a cycle, mark the node as such
            parentNode.AddNode(new Text("?? Circular reference detected", new Style(Color.Red)));
            return;
        }
        
        try
        {
            if (!fileRelationships.TryGetValue(parentPath, out var children) || children == null)
            {
                // Remove from visited before returning
                visitedPaths.Remove(parentPath);
                return;
            }

            // Get parent directory path in canonical form - safely
            string parentDirPath;
            try
            {
                parentDirPath = GetCanonicalPath(Path.GetDirectoryName(parentPath) ?? string.Empty);
            }
            catch
            {
                // Use a fallback if canonical path fails
                parentDirPath = Path.GetDirectoryName(parentPath)?.ToLowerInvariant() ?? string.Empty;
            }

            // Group files by directory for folder headers
            var filesByDirectory = new Dictionary<string, List<FileInfo>>(StringComparer.OrdinalIgnoreCase);
            
            // Process children only if they're not empty and not self-references
            foreach (var childPath in children.Where(path => 
                !string.IsNullOrEmpty(path) && 
                !string.Equals(path, parentPath, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var fileInfo = new FileInfo(childPath);
                    var dirPath = GetCanonicalPath(fileInfo.DirectoryName ?? string.Empty);
                    
                    if (!filesByDirectory.ContainsKey(dirPath))
                    {
                        filesByDirectory[dirPath] = new List<FileInfo>();
                    }
                    
                    filesByDirectory[dirPath].Add(fileInfo);
                }
                catch (Exception)
                {
                    // Skip problematic files
                    continue;
                }
            }
            
            // Process each directory group
            foreach (var dirEntry in filesByDirectory)
            {
                var dirPath = dirEntry.Key;
                var dirFiles = dirEntry.Value;
                
                // Skip parent directory as we don't need folder headers for it
                if (string.Equals(dirPath, parentDirPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Files in the same directory as parent - just add them directly
                    foreach (var fileInfo in dirFiles)
                    {
                        try
                        {
                            var fileName = fileInfo.Name;
                            bool isRepeated = repeatedFiles.Contains(fileInfo.FullName);
                            
                            string displayText = isRepeated && fileIds.TryGetValue(fileInfo.FullName, out int fileId) ?
                                $"{fileName} {string.Format(idFormat, fileId)}" : fileName;
                                
                            var style = isRepeated ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
                            var fileNode = parentNode.AddNode(new Text(displayText, style));
                            
                            // Skip recursion if this would create a cycle to an already visited path
                            if (fileRelationships.ContainsKey(fileInfo.FullName) && 
                                !visitedPaths.Contains(fileInfo.FullName))
                            {
                                // Create a new HashSet for this branch to isolate visited paths
                                AddChildrenToFolderGroupedTree(fileNode, fileInfo.FullName, fileRelationships,
                                    workspaceRoot, fileIds, repeatedFiles, directoryNodesMap, idFormat, 
                                    new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
                            }
                        }
                        catch (Exception)
                        {
                            // Skip problematic files
                            continue;
                        }
                    }
                }
                else
                {
                    try
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
                            DirectoryInfo dirInfo;
                            try
                            {
                                dirInfo = new DirectoryInfo(dirPath);
                            }
                            catch
                            {
                                // Fallback for invalid paths
                                dirInfo = new DirectoryInfo(Path.GetDirectoryName(parentPath) ?? ".");
                            }
                            
                            string folderName;
                            try
                            {
                                folderName = GetProjectRelativePath(dirInfo, workspaceRoot);
                            }
                            catch
                            {
                                // Fallback if path calculation fails
                                folderName = Path.GetFileName(dirPath);
                            }
                            
                            // Use [DIR] instead of emoji for better console compatibility
                            var folderText = new Text($"[DIR] {folderName}", new Style(Color.Yellow3));
                            folderNode = parentNode.AddNode(folderText);
                            directoryNodesMap[dirPath] = folderNode;
                        }
                        
                        // Add files under the folder header
                        foreach (var fileInfo in dirFiles)
                        {
                            try
                            {
                                var fileName = fileInfo.Name;
                                bool isRepeated = repeatedFiles.Contains(fileInfo.FullName);
                                
                                string displayText = isRepeated && fileIds.TryGetValue(fileInfo.FullName, out int fileId) ?
                                    $"{fileName} {string.Format(idFormat, fileId)}" : fileName;
                                    
                                var style = isRepeated ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
                                var fileNode = folderNode.AddNode(new Text(displayText, style));
                                
                                // Skip recursion if this would create a cycle to an already visited path
                                if (fileRelationships.ContainsKey(fileInfo.FullName) && 
                                    !visitedPaths.Contains(fileInfo.FullName))
                                {
                                    // Create a new HashSet for this branch to isolate visited paths
                                    AddChildrenToFolderGroupedTree(fileNode, fileInfo.FullName, fileRelationships,
                                        workspaceRoot, fileIds, repeatedFiles, directoryNodesMap, idFormat, 
                                        new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
                                }
                            }
                            catch (Exception)
                            {
                                // Skip problematic files
                                continue;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Skip problematic directories
                        continue;
                    }
                }
            }
            
            // Remove from visited paths when done with this branch
            visitedPaths.Remove(parentPath);
        }
        catch (Exception)
        {
            // If there's any error, make sure we remove from visited paths before returning
            visitedPaths.Remove(parentPath);
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
        string idFormat,
        HashSet<string>? visitedPaths = null)
    {
        // Initialize visited paths tracking if not provided
        visitedPaths ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Skip if already visited (cycle detection)
        if (!visitedPaths.Add(parentPath))
        {
            parentNode.AddNode(new Text("?? Circular reference detected", new Style(Color.Red)));
            return;
        }

        if (!fileRelationships.TryGetValue(parentPath, out var children))
        {
            visitedPaths.Remove(parentPath);
            return;
        }

        foreach (var childPath in children)
        {
            // Skip self-references
            if (string.Equals(childPath, parentPath, StringComparison.OrdinalIgnoreCase))
                continue;
                
            var fileName = Path.GetFileName(childPath);

            // Check if this file is repeated
            bool isRepeated = repeatedFiles.Contains(childPath);
            
            // Display text - only include ID if the file is repeated
            string displayText = isRepeated && fileIds.TryGetValue(childPath, out int fileId) ?
                $"{fileName} {string.Format(idFormat, fileId)}" : fileName;
            
            // Use yellow for repeated files
            var style = isRepeated ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
            
            var childNode = parentNode.AddNode(new Text(displayText, style));
            
            if (fileRelationships.ContainsKey(childPath) && !visitedPaths.Contains(childPath))
            {
                AddChildrenToRepeatsOnlyTree(childNode, childPath, fileRelationships, fileIds, repeatedFiles, idFormat,
                    new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
            }
        }
        
        visitedPaths.Remove(parentPath);
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
	    string idFormat,
	    HashSet<string>? visitedPaths = null)
	{
	    // Initialize visited paths tracking if not provided
        visitedPaths ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Skip if already visited (cycle detection)
        if (!visitedPaths.Add(parentPath))
        {
            parentNode.AddNode(new Text("?? Circular reference detected", new Style(Color.Red)));
            return;
        }
	
		if (!fileRelationships.TryGetValue(parentPath, out var children))
		{
		    visitedPaths.Remove(parentPath);
			return;
		}

		foreach (var childPath in children)
		{
		    // Skip self-references
            if (string.Equals(childPath, parentPath, StringComparison.OrdinalIgnoreCase))
                continue;
		    
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
			bool isRepeat = IsFileRepeatedInRelationships(childPath, fileRelationships);
			var style = isRepeat ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
			
			var childNode = parentNode.AddNode(new Text(displayText, style));
			
			if (fileRelationships.ContainsKey(childPath) && !visitedPaths.Contains(childPath))
            {
			    AddChildrenToSimpleTree(childNode, childPath, fileRelationships, fileIds, ref nextId, idFormat,
			        new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
            }
		}
		
		visitedPaths.Remove(parentPath);
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
	    string idFormat,
	    HashSet<string>? visitedPaths = null)
	{
	    // Initialize visited paths tracking if not provided
        visitedPaths ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Skip if already visited (cycle detection)
        if (!visitedPaths.Add(parentPath))
        {
            parentNode.AddNode(new Text("?? Circular reference detected", new Style(Color.Red)));
            return;
        }
        
		if (!fileRelationships.TryGetValue(parentPath, out var children))
		{
		    visitedPaths.Remove(parentPath);
			return;
		}

		foreach (var childPath in children)
		{
		    // Skip self-references
            if (string.Equals(childPath, parentPath, StringComparison.OrdinalIgnoreCase))
                continue;
		    
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
			bool isRepeat = IsFileRepeatedInRelationships(childPath, fileRelationships);
			var style = isRepeat ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
			
			var childNode = parentNode.AddNode(new Text(displayText, style));
			
			if (fileRelationships.ContainsKey(childPath) && !visitedPaths.Contains(childPath))
            {
			    AddChildrenToFolderTreeWithText(childNode, childPath, fileRelationships, workspaceRoot, fileIds, ref nextId, idFormat,
			        new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
            }
		}
		
		visitedPaths.Remove(parentPath);
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
	    string idFormat,
	    HashSet<string>? visitedPaths = null)
	{
	    // Initialize visited paths tracking if not provided
        visitedPaths ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Skip if already visited (cycle detection)
        if (!visitedPaths.Add(parentPath))
        {
            parentNode.AddNode(new Text("?? Circular reference detected", new Style(Color.Red)));
            return;
        }
        
		if (!fileRelationships.TryGetValue(parentPath, out var children))
		{
		    visitedPaths.Remove(parentPath);
			return;
		}

		foreach (var childPath in children)
		{
		    // Skip self-references
            if (string.Equals(childPath, parentPath, StringComparison.OrdinalIgnoreCase))
                continue;
                
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
			bool isRepeat = IsFileRepeatedInRelationships(childPath, fileRelationships);
			var style = isRepeat ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
			
			var text = new Text(displayText, style);
			var childNode = parentNode.AddNode(text);
			
			if (fileRelationships.ContainsKey(childPath) && !visitedPaths.Contains(childPath))
            {
			    AddChildrenToRelativePathTreeWithText(childNode, childPath, fileRelationships, baseDirectoryPath, fileIds, ref nextId, idFormat,
			        new HashSet<string>(visitedPaths, StringComparer.OrdinalIgnoreCase));
            }
		}
		
		visitedPaths.Remove(parentPath);
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
	            // Skip self-references
                if (string.Equals(child, currentFile, StringComparison.OrdinalIgnoreCase))
                    continue;
                    
	            // Only recurse if we haven't seen this file before
	            if (uniqueFiles.Add(child))
	            {
	                CountFilesRecursive(child);
	            }
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
        // Count file references to find repeated files
        var fileOccurrences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var repeatedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Use a queue for breadth-first traversal
        var queue = new Queue<string>();
        queue.Enqueue(rootFile.FullName);
        
        // Track visited files to avoid cycles
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        visited.Add(rootFile.FullName);
        
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
	
	/// <summary>
	/// Determines if a file is referenced multiple times in the file tree (non-recursive approach)
	/// </summary>
	private bool IsFileRepeatedInRelationships(string filePath, Dictionary<string, List<string>> fileRelationships)
	{
        // Simple non-recursive approach: count direct references to the file
        int count = 0;
        
        foreach (var entry in fileRelationships)
        {
            foreach (var child in entry.Value)
            {
                if (string.Equals(child, filePath, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    if (count > 1)
                        return true;
                }
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
	
	/// <summary>
	/// Helper method to get a canonical path with normalized directory separators
	/// </summary>
	private string GetCanonicalPath(string path)
	{
	    if (string.IsNullOrEmpty(path))
	        return string.Empty;
	        
	    try
	    {
	        // Use GetFullPath to normalize separators and resolve any relative segments
	        return Path.GetFullPath(path)
	            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
	            .ToLowerInvariant();
	    }
	    catch
	    {
	        // If there's an error (like invalid path), return the original path
	        return path.ToLowerInvariant();
	    }
	}
}