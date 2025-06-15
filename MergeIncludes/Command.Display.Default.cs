using MergeIncludes.Renderables;
using MergeIncludes.Services;
using Spectre.Console;

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
        Console.OutputEncoding = System.Text.Encoding.UTF8;        try
        {
            // Get the base directory of the root file
            var baseDirectory = rootFile.Directory ?? throw new InvalidOperationException("Root file directory cannot be null");

            // Create the root path display using TextPath
            var rootPath = new TextPath(baseDirectory.FullName)
                .RootStyle(Color.Blue)
                .SeparatorStyle(Color.Grey)
                .StemStyle(Color.DarkGreen)
                .LeafStyle(Color.Yellow);

            // Create the structure and reference view (content below the HR)
            var structureAndReferenceView = new Renderables.StructureAndReferenceView(rootFile, fileRelationships);

            // Create content with header, separator, and the structure/reference view
            var content = new Rows(
                rootPath,
                new Rule() { Style = Color.Grey },
                structureAndReferenceView
            );

            // Create panel with the content
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
