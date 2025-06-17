using Spectre.Console.Cli;
using System.ComponentModel;
using Throw;

namespace MergeIncludes;

/// <summary>
/// Defines the display mode for the file inclusion tree
/// </summary>
public enum TreeDisplayMode
{
    /// <summary>
    /// Default tree view with side-by-side folder structure and reference trees
    /// </summary>
    Default = 0,

    /// <summary>
    /// Tree showing file names with full paths
    /// </summary>
    FullPath = 1,

    /// <summary>
    /// Tree showing file paths relative to execution directory
    /// </summary>
    RelativePath = 2
}

public class MergeOptions : CommandSettings
{
    [Description("Trims leading and trailing empty lines. Default is true.")]
    [CommandOption("-t|--trim <TRIM_ENABLED>")]
    public bool? Trim { get; set; }

    [Description("Adds additional lines after the contents. Default is 1.")]
    [CommandOption("-p|--pad <LINE_PADDING>")]
    public int Padding { get; set; } = 1;

    [Description("Tree display mode: Default (side-by-side trees), FullPath (file paths list), RelativePath (paths relative to execution)")]
    [CommandOption("-d|--display <DISPLAY_MODE>")]
    public TreeDisplayMode DisplayMode { get; set; } = TreeDisplayMode.Default;

    [Description("Hide the full source path in tree display. Useful for deterministic output in tests.")]
    [CommandOption("--hide-path")]
    public bool HideFullPath { get; set; } = false;

    // NEW ENHANCEMENT: Show duplicate references
    [Description("Show all file references including duplicates with visual indicators. Default filters duplicates.")]
    [CommandOption("-D|--show-duplicates")]
    public bool ShowDuplicates { get; set; } = false;
}

public class Settings : MergeOptions
{
    [Description("The root file path to start from.")]
    [CommandArgument(0, "<ROOT_FILEPATH>")]
    public string? FilePath { get; set; }

    [Description("The output file path. If not provided, will output to <ROOT_FILEPATH>.merged.pine")]
    [CommandOption("-o|--out <OUTPUT_FILEPATH>")]
    public string? OutputFilePath { get; set; }

    [Description("Watch for changes and automatically re-merge. Use Ctrl+C to stop.")]
    [CommandOption("-w|--watch")]
    public bool Watch { get; set; } = false;

    [Description("Strict mode: fail if any include references cannot be resolved.")]
    [CommandOption("-s|--strict")]
    public bool Strict { get; set; } = false;

    public void Validate()
    {
        FilePath.ThrowIfNull("File path is required.");

        var file = new FileInfo(FilePath);
        file.Exists.ThrowIfFalse($"File '{FilePath}' does not exist.");

        if (!string.IsNullOrWhiteSpace(OutputFilePath))
        {
            var outputFile = new FileInfo(OutputFilePath);
            var outputDir = outputFile.Directory;
            outputDir.ThrowIfNull("Output directory is invalid.");

            if (!outputDir.Exists)
                outputDir.Create();
        }
    }
}
