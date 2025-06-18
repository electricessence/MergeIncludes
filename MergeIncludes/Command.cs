using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Extensions;
using System.Diagnostics.CodeAnalysis;
using Throw;

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

		var (success, files) = await Merge();

		if (!o.Watch) return success ? 0 : 1;

		using var cancelSource = new CancellationTokenSource();
		var token = cancelSource.Token;
		_ = Task.Run(async () => await _console.Status()
				.StartAsync("[turquoise2]Watching files for changes. (Press any key to stop watching.)[/]",
				async ctx =>
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

								// Use LinkableTextPath without forcing link creation, let it respect Windows Terminal detection
								_console.Write(PathLink.File(file));
							}

							_console.WriteLine();

							if (token.IsCancellationRequested)
								return;

							(_, files) = await Merge();
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

		async ValueTask<(bool Success, List<FileInfo> Files)> Merge()
		{
			var existed = outputFile.Exists;
			if (existed)
				outputFile.Attributes &= ~FileAttributes.ReadOnly;

			try
			{
				// Use the new MergeToMemoryAsync method to get the result
				var mergeResult = await MergeToMemoryAsync(o);

				if (!mergeResult.IsSuccess)
				{
					// Handle failure case - display tree first, then failure panel
					if (mergeResult.FileRelationships.Count > 0)
					{
						DisplayFileTrees(rootFile, mergeResult.FileRelationships, o.DisplayMode);
					}

					if (mergeResult.ErrorMessage?.Contains("Detected recursive reference") == true)
					{
						_console.Write(new Panel("[yellow]Circular Reference Detected[/]")
						{
							Header = new PanelHeader("[red]Failed to merge[/]"),
							Border = BoxBorder.Rounded
						});
					}
					else
					{
						_console.Write(new Panel($"[red]{mergeResult.ErrorMessage}[/]")
						{
							Header = new PanelHeader("[red]Failed to merge[/]"),
							Border = BoxBorder.Rounded
						});
					}

					return (false, mergeResult.ProcessedFiles);
				}

				// If we got here, everything was successful, so now write to the actual file
				using var output = outputFile.Open(FileMode.Create, FileAccess.Write);
				using var contentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(mergeResult.MergedContent!));
				await contentStream.CopyToAsync(output);

				// Create and render the file tree based on the selected display mode
				DisplayFileTrees(rootFile, mergeResult.FileRelationships, o.DisplayMode);

				// Helps prevent accidental editing by user.
				if (existed)
					outputFile.Attributes |= FileAttributes.ReadOnly;
				else
					outputFile.Attributes = FileAttributes.ReadOnly;

				// Use relative path for display, but keep full path for the link
				var outputPath = GetExecutionRelativeFilePath(outputFile);
				var mergePath = new LinkableTextPath(outputPath, outputFile.FullName)
					.RootStyle(Color.Blue)
					.SeparatorStyle(Color.Grey)
					.StemStyle(Color.DarkGreen)
					.LeafStyle(new Style(decoration: Decoration.Bold));
				_console.Write(new Panel(mergePath)
				{
					Header = new PanelHeader("[springgreen1]Successfully merged include references to:[/]"),
					Border = BoxBorder.Rounded
				});

				return (true, mergeResult.ProcessedFiles);
			}
			catch
			{
				// If there was an error, we don't modify the output file
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
			case TreeDisplayMode.RelativePath:
				DisplayRelativePathTree(rootFile, fileRelationships);
				break;
			default:
				DisplayDefaultTree(rootFile, fileRelationships);
				break;
		}
	}

	/// <summary>
	/// Display a tree with paths relative to the execution directory
	/// </summary>
	private void DisplayRelativePathTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Show context about the execution directory
		_console.MarkupLine($"[grey]Paths relative to execution directory: {GetExecutionDirectory()}[/]");

		// Create the tree with the root file using execution-relative path
		var rootPath = GetExecutionRelativeFilePath(rootFile);
		var tree = new Tree($"[blue]{rootPath}[/]");

		// Process children directly with the tree's nodes
		var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		// Start with the root file's children only - the root of the tree already shows the root file
		if (fileRelationships.TryGetValue(rootFile.FullName, out var rootChildren))
		{
			foreach (var childPath in rootChildren)
			{
				// Skip self references
				if (string.Equals(childPath, rootFile.FullName, StringComparison.OrdinalIgnoreCase))
					continue;

				try
				{
					var childFile = new FileInfo(childPath);
					var childRelativePath = GetExecutionRelativeFilePath(childFile);

					// Create a node for this child
					var childNode = tree.AddNode($"[cyan]{childRelativePath}[/]");

					// Process this child's children recursively
					if (fileRelationships.ContainsKey(childPath))
					{
						var newVisited = new HashSet<string>(visited, StringComparer.OrdinalIgnoreCase)
						{
							rootFile.FullName
						};
						AddChildrenToRelativePathTree(childNode, childPath, fileRelationships, newVisited);
					}
				}
				catch (Exception ex)
				{
					// Handle any errors gracefully
					tree.AddNode($"[red]Error processing {Path.GetFileName(childPath)}: {ex.Message}[/]");
				}
			}
		}

		_console.Write(tree);
	}

	/// <summary>
	/// Add children to a tree using execution-relative paths
	/// </summary>
	private static void AddChildrenToRelativePathTree(
		TreeNode parentNode,
		string parentPath,
		Dictionary<string, List<string>> fileRelationships,
		HashSet<string> visited)
	{
		if (visited.Contains(parentPath))
		{
			parentNode.AddNode("[red]⚠ Circular reference detected[/]");
			return;
		}

		visited.Add(parentPath);

		if (!fileRelationships.TryGetValue(parentPath, out var children))
			return;

		// Process each child path
		foreach (var childPath in children)
		{
			// Skip self references
			if (string.Equals(childPath, parentPath, StringComparison.OrdinalIgnoreCase))
				continue;

			try
			{
				var childFile = new FileInfo(childPath);
				var childRelativePath = GetExecutionRelativeFilePath(childFile);

				// Create a new node with the relative path
				var childNode = parentNode.AddNode($"[cyan]{childRelativePath}[/]");

				// Recursively add children if this file includes others
				if (fileRelationships.ContainsKey(childPath))
				{
					AddChildrenToRelativePathTree(childNode, childPath, fileRelationships,
						new HashSet<string>(visited, StringComparer.OrdinalIgnoreCase));
				}
			}
			catch (Exception ex)
			{
				// Handle any errors gracefully
				parentNode.AddNode($"[red]Error processing {Path.GetFileName(childPath)}: {ex.Message}[/]");
			}
		}

		visited.Remove(parentPath);
	}

	/// <summary>
	/// Performs the merge operation and returns the result without writing to a file.
	/// This method is primarily intended for testing and scenarios where you want 
	/// to get the merged content without file I/O.
	/// </summary>
	/// <param name="settings">The merge settings</param>
	/// <returns>A MergeResult containing the merged content or error information</returns>
	public static async Task<MergeResult> MergeToMemoryAsync(Settings settings)
	{
		settings.ThrowIfNull().OnlyInDebug();
		var rootFile = settings.GetRootFile();
		var list = new List<FileInfo>();
		var fileRelationships = new Dictionary<string, List<string>>();

		// Track which file is currently being processed for includes
		var currentlyProcessingFile = rootFile.FullName;

		// Use a MemoryStream to buffer the output
		using var memoryStream = new MemoryStream();
		using var writer = new StreamWriter(memoryStream);try
		{           // Process the files and write to the memory buffer
			await foreach (var line in rootFile.MergeIncludesAsync(settings, info =>
			{
				// Check if we're trying to include the output file (only relevant if settings has output file)
				if (settings.OutputFilePath != null)
				{
					var outputFile = settings.GetOutputFile(rootFile);
					if (info.FullName == outputFile.FullName)
						throw new InvalidOperationException("Attempting to include the output file.");
				}

				// Track all files for the flat list (used for watching)
				list.Add(info);

				// Record the parent-child relationship using the currently processing file
				if (!fileRelationships.TryGetValue(currentlyProcessingFile, out List<string>? value))
				{
					value = [];
					fileRelationships[currentlyProcessingFile] = value;
				}

				value.Add(info.FullName);
			}))			{
				await writer.WriteLineAsync(line);
			}

			await writer.FlushAsync();

			// Get the merged content as a string
			memoryStream.Position = 0;
			using var reader = new StreamReader(memoryStream);
			var mergedContent = await reader.ReadToEndAsync();

			return MergeResult.Success(mergedContent, list, fileRelationships);
		}
		catch (InvalidOperationException ex) when (ex.Message.Contains("Detected recursive reference"))
		{
			return MergeResult.Failure(ex.Message, list, fileRelationships);
		}
		catch (Exception ex)
		{
			return MergeResult.Failure(ex.Message, list, fileRelationships);
		}
	}
}
