using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;

namespace MergeIncludes.TreeBuilders;

/// <summary>
/// Utility class for building reference dependency trees using native Spectre.Console Tree
/// </summary>
public static class ReferenceTreeBuilder
{
    /// <summary>
    /// Create a reference tree with clickable file links using Markup
    /// </summary>
    public static Tree Create(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        // Create root file as clickable link using Markup
        var rootFileUrl = CreateFileUrl(rootFile.FullName);
        var rootMarkup = new Markup($"[yellow bold link={rootFileUrl}]{rootFile.Name.EscapeMarkup()}[/]");

        var tree = new Tree(rootMarkup);
        var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Build the dependency tree recursively
        BuildTreeRecursive(tree, rootFile.FullName, fileRelationships, processedFiles, 0);

        return tree;
    }

    /// <summary>
    /// Creates a properly encoded file URL for use in Markup links
    /// </summary>
    private static string CreateFileUrl(string filePath)
    {
        // Convert to forward slashes and encode spaces and other special characters
        var encodedPath = Uri.EscapeDataString(filePath.Replace('\\', '/'));
        return $"file:///{encodedPath}";
    }

    /// <summary>
    /// Recursively builds the reference tree by adding child nodes
    /// </summary>
    private static void BuildTreeRecursive(Tree tree, string filePath,
        Dictionary<string, List<string>> fileRelationships, HashSet<string> processedFiles, int depth)
    {
        if (processedFiles.Contains(filePath) || !fileRelationships.TryGetValue(filePath, out var children))
            return;

        processedFiles.Add(filePath);

        foreach (var childPath in children)
        {
            if (File.Exists(childPath) && !processedFiles.Contains(childPath))
            {
                var childFile = new FileInfo(childPath);
                var childFileUrl = CreateFileUrl(childPath);
                var color = depth == 0 ? "lime" : "cyan1";
                var childMarkup = new Markup($"[{color} link={childFileUrl}]{childFile.Name.EscapeMarkup()}[/]");

                var childNode = tree.AddNode(childMarkup);

                // Recursively build from this child node
                BuildTreeNodeRecursive(childNode, childPath, fileRelationships, processedFiles, depth + 1);
            }
        }
    }

    /// <summary>
    /// Recursively builds child nodes in the reference tree
    /// </summary>
    private static void BuildTreeNodeRecursive(TreeNode parentNode, string filePath,
        Dictionary<string, List<string>> fileRelationships, HashSet<string> processedFiles, int depth)
    {
        if (processedFiles.Contains(filePath) || !fileRelationships.TryGetValue(filePath, out var children))
            return;

        processedFiles.Add(filePath);

        foreach (var childPath in children)
        {
            if (File.Exists(childPath) && !processedFiles.Contains(childPath))
            {
                var childFile = new FileInfo(childPath);
                var childFileUrl = CreateFileUrl(childPath);
                var color = depth <= 1 ? "lime" : "cyan1";
                var childMarkup = new Markup($"[{color} link={childFileUrl}]{childFile.Name.EscapeMarkup()}[/]");

                var childNode = parentNode.AddNode(childMarkup);

                // Continue recursively
                BuildTreeNodeRecursive(childNode, childPath, fileRelationships, processedFiles, depth + 1);
            }
        }
    }
}
