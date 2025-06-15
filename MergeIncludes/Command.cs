using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using Throw;
using Spectre.Console.Extensions;

namespace MergeIncludes;

/// <summary>
/// Command to merge files with included references
/// </summary>
public sealed partial class CombineCommand(IAnsiConsole console)
	: AsyncCommand<Settings>
{
	private readonly IAnsiConsole _console = console ?? throw new ArgumentNullException(nameof(console));

	public CombineCommand() : this(AnsiConsole.Console)
	{
	}

	public override ValidationResult Validate(
		[NotNull] CommandContext context,
		[NotNull] Settings settings)
	{
		var rootPath = settings.RootFilePath;
		if (string.IsNullOrWhiteSpace(rootPath)) return ValidationResult.Error("Root file path can't be empty or white space.");
		if (!File.Exists(rootPath)) return ValidationResult.Error("Root file not found.");

		settings.Trim ??= true;

		return base.Validate(context, settings);
	}

	public override async Task<int> ExecuteAsync(
		[NotNull] CommandContext __,
		[NotNull] Settings o)
	{
		o.ThrowIfNull().OnlyInDebug();
		var rootFile = o.GetRootFile();
		var outputFile = o.GetOutputFile(rootFile);

		var files = await Merge();

		if (!o.Watch) return 0;

		using var cancelSource = new CancellationTokenSource();
		var token = cancelSource.Token;
		_ = Task.Run(async () =>
			await _console.Status()
				.StartAsync("[turquoise2]Watching files for changes. (Press any key to stop watching.)[/]",
				async _ =>
				{
					while (!token.IsCancellationRequested)
					{
						try
						{
							int count = 0;
							await foreach (var file in FileWatcher.WatchAsync(files, 1000, token))
							{
								if (count++ == 0)
								{
									var now = DateTimeOffset.Now;
									_console.MarkupLine($"[yellow]Changes detected:[/] ({now:d} {now:T})");
								}

								// Use LinkableTextPath instead of TextPath to make the path clickable
								_console.Write(new LinkableTextPath(file, true));
							}

							_console.WriteLine();

							if (token.IsCancellationRequested)
								return;

							files = await Merge();
						}
						catch (OperationCanceledException)
						{
							return;
						}
						catch (FileNotFoundException ex)
						{
							ex.WriteToConsole(console: _console);
						}
						catch (Exception ex)
						{
							ex.WriteToConsole(console: _console);
						}
					}
				}));

		Console.ReadKey();
		cancelSource.Cancel();

		return 0;

		async ValueTask<List<FileInfo>> Merge()
		{
			var existed = outputFile.Exists;
			if (existed)
				outputFile.Attributes &= ~FileAttributes.ReadOnly;

			var list = new List<FileInfo>();
			var fileRelationships = new Dictionary<string, List<string>>();

			// Stack to track the current inclusion path
			var includeStack = new Stack<string>();
			includeStack.Push(rootFile.FullName);

			// Use a MemoryStream to buffer the output instead of writing directly to the file
			using var memoryStream = new MemoryStream();
			using var writer = new StreamWriter(memoryStream);

			try
			{
				// Process the files and write to the memory buffer
				await foreach (var line in rootFile.MergeIncludesAsync(o, info =>
				{
					if (info.FullName == outputFile.FullName)
						throw new InvalidOperationException("Attempting to include the output file.");

					// Track all files for the flat list (used for watching)
					list.Add(info);

					// Get the parent file that included this file
					var parentFile = includeStack.Peek();

					// Record the parent-child relationship
					if (!fileRelationships.TryGetValue(parentFile, out List<string>? value))
					{
						value = [];
						fileRelationships[parentFile] = value;
					}

					value.Add(info.FullName);

					// Push this file onto the stack as it might include other files
					includeStack.Push(info.FullName);

					// Note: We don't pop from the stack here because we don't know when the
					// file processing is complete. The stack will accumulate the current branch
					// of file inclusions.
				}))
				{
					await writer.WriteLineAsync(line);

					// If we detect a line without an include statement, we can assume we've moved back up
					// in the file hierarchy by one level (this is a simplification but generally works)
					if (!line.Contains("#include") && !line.Contains("#require") && includeStack.Count > 1)
					{
						includeStack.Pop();
					}
				}

				await writer.FlushAsync();

				// If we got here, everything was successful, so now write to the actual file
				using var output = outputFile.Open(FileMode.Create, FileAccess.Write);
				memoryStream.Position = 0;
				await memoryStream.CopyToAsync(output);

				// Create and render the file tree based on the selected display mode
				DisplayFileTrees(rootFile, fileRelationships, o.DisplayMode);

				// Helps prevent accidental editing by user.
				if (existed)
					outputFile.Attributes |= FileAttributes.ReadOnly;
				else
					outputFile.Attributes = FileAttributes.ReadOnly;

				// Use LinkableTextPath instead of TextPath for the output file path
				var mergePath = new LinkableTextPath(outputFile.FullName, true);
				_console.Write(new Panel(mergePath)
				{
					Header = new PanelHeader("[springgreen1]Successfully merged include references to:[/]"),
					Border = BoxBorder.Rounded
				});

				return list;
			}
			catch
			{
				// If there was an error, we don't modify the output file
				// since we only wrote to the memory stream
				throw;
			}
		}
	}

	/// <summary>
	/// Entry point for displaying file trees according to the selected mode
	/// </summary>
	private void DisplayFileTrees(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships, TreeDisplayMode mode)
	{
		// Display trees based on the selected mode
		switch (mode)
		{
			case TreeDisplayMode.Default:
				DisplayDefaultTree(rootFile, fileRelationships);
				break;
			case TreeDisplayMode.FullPath:
				DisplayFullPathTree(rootFile, fileRelationships);
				break;
			default:
				DisplayDefaultTree(rootFile, fileRelationships);
				break;
		}
	}
}
