using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MergeIncludes.TreeBuilders;

/// <summary>
/// Utility class for building folder structure trees using native Spectre.Console Tree
/// </summary>
public static class FolderTreeBuilder
{
    /// <summary>
    /// Create a folder tree from a root file and its dependencies
    /// </summary>
    public static Tree FromDependencies(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        var allFiles = new List<FileInfo> { rootFile };
        CollectAllFilesRecursive(rootFile.FullName, fileRelationships, allFiles);
        return CreateFolderTree(rootFile.Directory!, allFiles);
    }

    /// <summary>
    /// Create a folder tree for the given files
    /// </summary>
    public static Tree Create(DirectoryInfo baseDirectory, IEnumerable<FileInfo> files)
    {
        return CreateFolderTree(baseDirectory, files);    }

    /// <summary>
    /// Create a folder tree for the given files
    /// </summary>
    private static Tree CreateFolderTree(DirectoryInfo baseDirectory, IEnumerable<FileInfo> files)
    {
        // Create root folder as clickable link with folder icon
        var rootFolderName = $"📁 {baseDirectory.Name}";
        var rootText = new Text(rootFolderName, new Style(foreground: Color.Blue, decoration: Decoration.Bold, link: baseDirectory.FullName));

        var tree = new Tree(rootText);

        // Group files by their directories
        var filesByDirectory = files.GroupBy(f => f.Directory!.FullName).ToList();

        foreach (var directoryGroup in filesByDirectory)
        {
            var directory = new DirectoryInfo(directoryGroup.Key);
            var relativePath = Path.GetRelativePath(baseDirectory.FullName, directory.FullName);

            if (relativePath == "." || string.IsNullOrEmpty(relativePath))
            {
                // Files in root directory - add directly
                foreach (var file in directoryGroup.OrderBy(f => f.Name))
                {
                    var fileText = new Text(file.Name, new Style(foreground: Color.Green, link: file.FullName));
                    tree.AddNode(fileText);
                }
            }
            else
            {
                // Files in subdirectory - add folder then files with folder icon
                var folderName = $"📁 {directory.Name}";
                var folderText = new Text(folderName, new Style(foreground: Color.Cyan1, decoration: Decoration.Bold, link: directory.FullName));
                var folderNode = tree.AddNode(folderText);

                foreach (var file in directoryGroup.OrderBy(f => f.Name))
                {
                    var fileText = new Text(file.Name, new Style(foreground: Color.Green, link: file.FullName));
                    folderNode.AddNode(fileText);
                }
            }
        }

        return tree;
    }

    private static void CollectAllFilesRecursive(string filePath, Dictionary<string, List<string>> fileRelationships, List<FileInfo> allFiles)
    {
        if (!fileRelationships.TryGetValue(filePath, out var children))
            return;

        foreach (var childPath in children)
        {
            if (File.Exists(childPath) && !allFiles.Any(f => f.FullName.Equals(childPath, StringComparison.OrdinalIgnoreCase)))
            {
                allFiles.Add(new FileInfo(childPath));
                CollectAllFilesRecursive(childPath, fileRelationships, allFiles);
            }
        }
    }
}
