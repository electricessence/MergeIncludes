using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using MergeIncludes.Services;

namespace MergeIncludes;

/// <summary>
/// Default display mode - side-by-side folder structure and reference trees
/// </summary>
partial class CombineCommand
{
    /// <summary>
    /// Displays two trees side by side: folder structure on left, reference structure on right
    /// </summary>
    private void DisplayDefaultTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
    {
        try
        {
            // Get the base directory of the root file
            var baseDirectory = rootFile.Directory ?? throw new InvalidOperationException("Root file directory cannot be null");

            // Create the root path display using TextPath
            var rootPath = new TextPath(baseDirectory.FullName)
                .RootStyle(new Style(foreground: Color.Blue))
                .SeparatorStyle(new Style(foreground: Color.Grey))
                .StemStyle(new Style(foreground: Color.DarkGreen))
                .LeafStyle(new Style(foreground: Color.Yellow));

            // Build side-by-side trees using the service
            var treeBuilder = new DefaultTreeBuilder();
            var (folderLines, referenceLines) = treeBuilder.BuildSideBySideTrees(rootFile, fileRelationships);

            // Create a table to align the trees side by side
            var table = new Table()
                .Border(TableBorder.None)
                .HideHeaders()
                .AddColumn(new TableColumn(""))
                .AddColumn(new TableColumn(""));

            // Add the tree lines as table rows
            for (int i = 0; i < Math.Max(folderLines.Count, referenceLines.Count); i++)
            {
                var folderLine = i < folderLines.Count ? folderLines[i] : "";
                var referenceLine = i < referenceLines.Count ? referenceLines[i] : "";

                table.AddRow(new Text(folderLine), new Text(referenceLine));
            }

            // Create content with header, separator, and aligned trees
            var content = new Rows(
                new Panel(rootPath) { Border = BoxBorder.None },
                new Rule() { Style = Style.Parse("grey") },
                table
            );

            // Create content with just the table (no extra spacing)
            var panel = new Panel(content)
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 0, 1, 0),
                Expand = false
            };
            _console.Write(panel);
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[red]Error displaying default tree:[/] {ex.Message}");
        }
    }
}
