using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;

namespace MergeIncludes.TreeBuilders;

/// <summary>
/// Simple, testable reference tree builder that follows basic parent-child relationship rules
/// </summary>
public static class SimpleReferenceTreeBuilder
{
    /// <summary>
    /// Builds a reference tree from file relationships
    /// </summary>    /// <param name="rootFilePath">The root file path</param>    /// <param name="fileRelationships">Dictionary mapping parent files to their children</param>    /// <returns>A tree structure representing the reference hierarchy</returns>
    public static TreeMinimalWidth BuildReferenceTree(string rootFilePath, Dictionary<string, List<string>> fileRelationships)
    {
        var rootFileName = Path.GetFileName(rootFilePath);
        var rootLink = HyperLink.For(rootFilePath, rootFileName, new Style(Color.Yellow));
        var tree = new TreeMinimalWidth(rootLink);
        
        // First pass: count how many times each file appears
        var fileCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        CountFileOccurrences(rootFilePath, fileRelationships, fileCounts, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        
        // Second pass: assign IDs to files that appear more than once  
        var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var nextId = 1;
        foreach (var kvp in fileCounts.Where(x => x.Value > 1))
        {
            fileIds[kvp.Key] = nextId++;
        }
        
        // Third pass: build the tree, tracking first occurrence of each file
        var firstOccurrence = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        
        // Build the tree recursively starting from the root
        BuildTreeRecursive(tree, rootFilePath, fileRelationships, new HashSet<string>(), fileIds, firstOccurrence);
        
        return tree;
    }
    
    /// <summary>
    /// Count how many times each file appears in the entire tree
    /// </summary>
    private static void CountFileOccurrences(string filePath, Dictionary<string, List<string>> fileRelationships, 
        Dictionary<string, int> fileCounts, HashSet<string> visited)
    {
        if (visited.Contains(filePath))
            return;
            
        visited.Add(filePath);
        
        if (fileRelationships.TryGetValue(filePath, out var children))
        {
            foreach (var childPath in children)
            {
                var childFileName = Path.GetFileName(childPath);
                fileCounts.TryGetValue(childFileName, out var currentCount);
                fileCounts[childFileName] = currentCount + 1;
                
                CountFileOccurrences(childPath, fileRelationships, fileCounts, visited);
            }
        }
    }
      
    /// <summary>
    /// Recursively builds the tree structure
    /// </summary>
    private static void BuildTreeRecursive(IHasTreeNodes parentNode, string currentFilePath, 
        Dictionary<string, List<string>> fileRelationships, HashSet<string> visitedInCurrentBranch,
        Dictionary<string, int> fileIds, Dictionary<string, bool> firstOccurrence)
    {
        // Prevent infinite recursion in current branch
        if (visitedInCurrentBranch.Contains(currentFilePath))
            return;
            
        // Add to visited for this branch
        visitedInCurrentBranch.Add(currentFilePath);
        
        // Get children of current file
        if (fileRelationships.TryGetValue(currentFilePath, out var children))
        {
            foreach (var childPath in children)
            {                var childFileName = Path.GetFileName(childPath);
                
                // Create display text based on whether this file has duplicates and if it's the first occurrence
                var displayText = CreateDisplayText(childPath, childFileName, fileIds, firstOccurrence);
                var childNode = parentNode.AddNode(displayText);
                
                // Recursively build children with a new visited set for each branch
                BuildTreeRecursive(childNode, childPath, fileRelationships, 
                    new HashSet<string>(visitedInCurrentBranch), fileIds, firstOccurrence);
            }
        }
        
        // Remove from visited when leaving this branch
        visitedInCurrentBranch.Remove(currentFilePath);
    }    /// <summary>
    /// Creates the display text for a file, including duplicate ID numbering and styling
    /// </summary>
    private static IRenderable CreateDisplayText(string filePath, string fileName, Dictionary<string, int> fileIds, Dictionary<string, bool> firstOccurrence)
    {
        // Check if this file has duplicates (has an assigned ID)
        if (fileIds.TryGetValue(fileName, out var fileId))
        {
            // This file appears multiple times - ALL instances get the same ID
            if (!firstOccurrence.ContainsKey(fileName))
            {
                // First occurrence - show ID, normal color
                firstOccurrence[fileName] = true;
                var displayText = $"{fileName} [{fileId}]";
                return HyperLink.For(filePath, displayText, new Style(Color.Cyan1));
            }
            else
            {
                // Subsequent occurrences - show same ID, darker color
                var displayText = $"{fileName} [{fileId}]";
                return HyperLink.For(filePath, displayText, new Style(Color.Grey50));
            }
        }
        else
        {
            // File appears only once - normal color, no ID
            return HyperLink.For(filePath, fileName, new Style(Color.Cyan1));
        }
    }
}
