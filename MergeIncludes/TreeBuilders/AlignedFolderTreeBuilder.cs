using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    public static IRenderable FromDependencies(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
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
        public List<ReferenceNode> Children { get; set; } = new();
        public int Level { get; set; } = 0;
    }

    /// <summary>
    /// Build the reference structure to understand what needs to align
    /// </summary>
    private static List<ReferenceNode> BuildReferenceStructure(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        var structure = new List<ReferenceNode>();
        var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Add root file
        var rootNode = new ReferenceNode
        {
            FilePath = rootFile.FullName,
            FileName = rootFile.Name,
            Directory = rootFile.Directory!,
            Level = 0
        };
        structure.Add(rootNode);

        // Recursively build the structure
        BuildReferenceStructureRecursive(rootNode, fileRelationships, processedFiles);

        // Flatten to a linear list for alignment
        var flatList = new List<ReferenceNode>();
        FlattenReferenceStructure(structure, flatList);
        
        return flatList;
    }

    /// <summary>
    /// Recursively build the reference structure
    /// </summary>
    private static void BuildReferenceStructureRecursive(
        ReferenceNode parentNode, 
        Dictionary<string, List<string>> fileRelationships, 
        HashSet<string> processedFiles)
    {
        if (processedFiles.Contains(parentNode.FilePath) || 
            !fileRelationships.TryGetValue(parentNode.FilePath, out var dependencies))
            return;

        processedFiles.Add(parentNode.FilePath);

        foreach (var dependency in dependencies)
        {
            if (File.Exists(dependency) && !processedFiles.Contains(dependency))
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
                BuildReferenceStructureRecursive(childNode, fileRelationships, processedFiles);
            }
        }
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
    }

    /// <summary>
    /// Create an aligned folder tree based on the reference structure
    /// </summary>
    private static IRenderable CreateAlignedFolderTree(FileInfo rootFile, List<ReferenceNode> referenceStructure)
    {        // Create root folder - blue color, no bold
        var rootFolderName = $"📁 {rootFile.Directory!.Name}";
        var rootStyle = IsWindowsTerminal 
            ? new Style(foreground: Color.Blue, link: rootFile.Directory.FullName)
            : new Style(foreground: Color.Blue);
        
        var rootText = new Text(rootFolderName, rootStyle);
        var tree = new TreeMinimalWidth(rootText);

        // Track which directories we've already shown
        var shownDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        shownDirectories.Add(rootFile.Directory.FullName);        // Process each reference node and create proper tree structure with child nodes
        ProcessReferenceNodesForHierarchy(tree, rootFile.Directory, referenceStructure.Skip(1), shownDirectories);

        return tree;
    }    /// <summary>
    /// Process reference nodes and create proper tree hierarchy with child nodes under folders
    /// </summary>
    private static void ProcessReferenceNodesForHierarchy(
        TreeMinimalWidth tree, 
        DirectoryInfo baseDirectory,
        IEnumerable<ReferenceNode> referenceNodes,
        HashSet<string> shownDirectories)
    {
        // Group reference nodes by directory to avoid creating multiple entries for same folder
        var nodesByDirectory = referenceNodes
            .GroupBy(rn => rn.Directory.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        
        foreach (var directoryGroup in nodesByDirectory)
        {
            var directory = new DirectoryInfo(directoryGroup.Key);
            var filesInDirectory = directoryGroup.Count();
            
            if (directory.FullName.Equals(baseDirectory.FullName, StringComparison.OrdinalIgnoreCase))
            {
                // Files are in the same directory as root - add single spacer
                var spacer = new Text("\u00A0", new Style(foreground: Color.Default));
                tree.AddNode(spacer);
            }
            else if (!shownDirectories.Contains(directory.FullName))
            {
                // New directory - show it with relative path if outside base directory
                // Subfolders are green, no bold
                var folderName = GetFolderDisplayName(baseDirectory, directory);
                var folderStyle = IsWindowsTerminal
                    ? new Style(foreground: Color.Green, link: directory.FullName)
                    : new Style(foreground: Color.Green);                var folderText = new Text(folderName, folderStyle);
                var folderNode = tree.AddNode(folderText);
                
                // Add (filesInDirectory - 1) fake children to create proper tree structure
                // First file aligns with folder, each additional file needs an extra line
                for (int i = 0; i < filesInDirectory - 1; i++)
                {
                    var fakeChild = new Text("\u00A0", new Style(foreground: Color.Default));
                    folderNode.AddNode(fakeChild);
                }
                
                shownDirectories.Add(directory.FullName);
            }
        }
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
