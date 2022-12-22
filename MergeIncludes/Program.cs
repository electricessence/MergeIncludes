using CommandLine;
using MergeIncludes;
using Throw;

await Parser.Default
	.ParseArguments<Options>(args)
	.WithParsedAsync(async o =>
	{
		o.ThrowIfNull().OnlyInDebug();
		var rootFile = o.GetRootFile();
		var outputFile = o.GetOutputFile(rootFile);

		Console.WriteLine("Files read from:");
		using var output = outputFile.OpenWrite();
		using var writer = new StreamWriter(output);
		await foreach (var line in rootFile.MergeIncludesAsync(info =>
		{
			if (info.FullName == outputFile.FullName)
				throw new InvalidOperationException("Attempting to include the output file.");

			Console.WriteLine(info.FullName);
		}))
		{
			await writer.WriteAsync(line);
		}

		Console.WriteLine();
		Console.WriteLine("Successfully merged include references to:");
		Console.WriteLine(outputFile.FullName);
	});

class Options
{
	[Option('r', "root", Required = true, HelpText = "The root file path to start from.")]
	public string? RootFilePath { get; set; }

	[Option('o', "out", Required = false, HelpText = "The file to render the results to.")]
	public string? OutputFilePath { get; set; }

	public FileInfo GetRootFile()
	{
		RootFilePath.ThrowIfNull().OnlyInDebug();
		RootFilePath.Throw().IfEmpty().OnlyInDebug();
		RootFilePath.Throw().IfWhiteSpace();
		var file = new FileInfo(RootFilePath);
		if (!file.Exists)
			throw new FileNotFoundException(RootFilePath);
		return file;
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