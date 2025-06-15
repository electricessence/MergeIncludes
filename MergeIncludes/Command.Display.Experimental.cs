using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MergeIncludes.Renderables;
using MergeIncludes.TreeBuilders;

namespace MergeIncludes;

/// <summary>
/// Enhanced display mode with clickable tree navigation
/// </summary>
partial class CombineCommand
{
    /// <summary>
    /// Enhanced display mode featuring clickable file and folder links
    /// </summary>
    private void DisplayExperimentalTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        try
        {
            // Create trees using native Spectre.Console Tree widgets
            var folderTree = FolderTreeBuilder.FromDependencies(rootFile, fileRelationships);
            var referenceTree = ReferenceTreeBuilder.Create(rootFile, fileRelationships);

            // Create side-by-side table (explicit width constraints)
            var sideBySideTable = new Table()
                .Border(TableBorder.None)
                .HideHeaders()
                .AddColumn(new TableColumn("").Width(60))
                .AddColumn(new TableColumn("").Width(60))
                .AddRow(folderTree, referenceTree);

            // Wrap in a panel that doesn't expand to full width
            var panel = new Panel(sideBySideTable)
            {
                Header = new PanelHeader("📁 Project Structure & Dependencies"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 0, 1, 0),
                Expand = false  // Key: don't expand to full width
            };

            _console.Write(panel);
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[red]Error displaying tree structure:[/] {ex.Message}");
        }
    }
}
