using Spectre.Console;
using Spectre.Console.Extensions;

namespace MergeIncludes;

/// <summary>
/// Enhanced display utilities with duplicate reference tracking
/// </summary>
partial class CombineCommand
{
    /// <summary>
    /// Tracks file references for duplicate detection
    /// </summary>
    private class ReferenceTracker
    {
        private readonly Dictionary<string, int> _fileIds = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _referenceCounts = new(StringComparer.OrdinalIgnoreCase);
        private int _nextId = 1;

        public int GetOrAssignId(string filePath)
        {
            if (!_fileIds.TryGetValue(filePath, out var id))
            {
                id = _nextId++;
                _fileIds[filePath] = id;
                _referenceCounts[filePath] = 1;
            }
            else
            {
                _referenceCounts[filePath]++;
            }
            return id;
        }

        public bool IsFirstReference(string filePath)
        {
            return _referenceCounts.GetValueOrDefault(filePath) == 1;
        }

        public int GetReferenceCount(string filePath)
        {
            return _referenceCounts.GetValueOrDefault(filePath);
        }
    }

    /// <summary>
    /// Creates enhanced reference tree with duplicate tracking
    /// </summary>
    private static Tree CreateEnhancedReferenceTree(
        FileInfo rootFile,
        Dictionary<string, List<string>> fileRelationships,
        bool showDuplicates)
    {
        var rootMarkup = HyperLink.For(rootFile);
        var tree = new Tree(rootMarkup);

        if (showDuplicates)
        {
            var tracker = new ReferenceTracker();
            var processedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            BuildEnhancedReferenceTreeRecursive(tree, rootFile.FullName, fileRelationships, tracker, processedPaths);
        }
        else
        {
            // Use existing logic for backward compatibility
            var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            BuildReferenceTreeRecursive(tree, rootFile.FullName, fileRelationships, processedFiles);
        }

        return tree;
    }

    /// <summary>
    /// Enhanced recursive tree building with duplicate visualization
    /// </summary>
    private static void BuildEnhancedReferenceTreeRecursive(
        Tree tree,
        string filePath,
        Dictionary<string, List<string>> fileRelationships,
        ReferenceTracker tracker,
        HashSet<string> processedPaths)
    {
        if (!fileRelationships.TryGetValue(filePath, out var dependencies))
            return;

        foreach (var dependency in dependencies)
        {
            if (!File.Exists(dependency))
                continue;

            var dependencyName = Path.GetFileName(dependency);
            var referenceId = tracker.GetOrAssignId(dependency);
            var isFirstReference = tracker.IsFirstReference(dependency);

            // Create markup with reference ID and appropriate styling
            var displayName = $"{dependencyName} [{referenceId}]";
            var markup = CreateDuplicateAwareMarkup(dependency, displayName, isFirstReference);
            var childNode = tree.AddNode(markup);

            if (isFirstReference)
            {
                // First reference: show full dependency tree
                BuildEnhancedReferenceTreeRecursive(childNode, dependency, fileRelationships, tracker, processedPaths);
            }
            else
            {
                // Duplicate reference: show collapsed indicator
                var referenceNote = $"[dim]→ See reference [{referenceId}][/]";
                childNode.AddNode(referenceNote);
            }
        }
    }

    /// <summary>
    /// Creates markup with appropriate styling for duplicate awareness
    /// </summary>
    private static Markup CreateDuplicateAwareMarkup(string filePath, string displayName, bool isFirstReference)
    {
        if (isFirstReference)
        {
            // First reference: use distinct color (Yellow/Orange)
            return new Markup($"[bold yellow]{displayName.EscapeMarkup()}[/]");
        }
        else
        {
            // Duplicate reference: use dimmed gray
            return new Markup($"[dim grey]{displayName.EscapeMarkup()}[/]");
        }
    }

    /// <summary>
    /// Backward compatible reference tree builder (existing logic)
    /// </summary>
    private static void BuildReferenceTreeRecursive(
        Tree tree,
        string filePath,
        Dictionary<string, List<string>> fileRelationships,
        HashSet<string> processedFiles)
    {
        if (processedFiles.Contains(filePath) || !fileRelationships.TryGetValue(filePath, out var dependencies))
            return;

        processedFiles.Add(filePath);
        foreach (var dependency in dependencies)
        {
            if (File.Exists(dependency) && !processedFiles.Contains(dependency))
            {
                var dependencyName = Path.GetFileName(dependency);
                var dependencyMarkup = HyperLink.For(dependency, dependencyName);
                var childNode = tree.AddNode(dependencyMarkup);

                // Recursively build dependencies of this dependency
                BuildReferenceTreeRecursive(childNode, dependency, fileRelationships, processedFiles);
            }
        }
    }
}
