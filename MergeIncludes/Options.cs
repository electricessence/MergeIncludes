using CommandLine;
using Throw;

namespace MergeIncludes;

class Options
{
	[Option('r', "root", Required = true, HelpText = "The root file path to start from.")]
	public string? RootFilePath { get; set; }

	[Option('o', "out", Required = false, HelpText = "The file to render the results to.")]
	public string? OutputFilePath { get; set; }

	[Option('w', "watch", Required = false, HelpText = "Will keep this running to wait for changes.")]
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