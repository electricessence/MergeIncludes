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
    Default = 0,    /// <summary>
                    /// Tree showing file names with full paths
                    /// </summary>
    FullPath = 1
}

public class MergeOptions : CommandSettings
{
    [Description("Trims leading and trailing empty lines. Default is true.")]
    [CommandOption("-t|--trim <TRIM_ENABLED>")]
    public bool? Trim { get; set; }

    [Description("Adds additional lines after the contents. Default is 1.")]
    [CommandOption("-p|--pad <LINE_PADDING>")]
    public int Padding { get; set; } = 1; [Description("Tree display mode: Default (side-by-side trees), FullPath (file paths list), or Experimental (testing new renderables)")]
    [CommandOption("-d|--display <DISPLAY_MODE>")]
    public TreeDisplayMode DisplayMode { get; set; } = TreeDisplayMode.Default;
}

public class Settings : MergeOptions
{
    [Description("The root file path to start from.")]
    [CommandArgument(0, "<ROOT_FILEPATH>")]
    public string RootFilePath { get; set; } = "";

    [Description("The file to render the results to.")]
    [CommandOption("-o|--out <OUTPUT_FILEPATH>")]
    public string OutputFilePath { get; set; } = "";

    [Description("Will keep this running to wait for changes.")]
    [CommandOption("-w|--watch")]
    public bool Watch { get; set; }

    public FileInfo GetRootFile()
    {
        RootFilePath.ThrowIfNull().OnlyInDebug();
        RootFilePath.Throw().IfEmpty().OnlyInDebug();
        RootFilePath.Throw().IfWhiteSpace();
        return new FileInfo(RootFilePath);
    }

    public FileInfo GetOutputFile(FileInfo root)
    {
        root.ThrowIfNull().OnlyInDebug();
        if (string.IsNullOrEmpty(OutputFilePath))
        {
            DirectoryInfo dir = root.Directory.ThrowIfNull();
            return new FileInfo(Path.Combine(
                dir.FullName,
                $"{Path.GetFileNameWithoutExtension(root.Name)}.merged{root.Extension}"));
        }

        return new FileInfo(OutputFilePath);
    }
}
