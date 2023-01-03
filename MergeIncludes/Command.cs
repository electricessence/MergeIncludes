using Spectre.Console.Cli;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;
using Throw;
using Spectre.Console.Extensions;

namespace MergeIncludes;

internal sealed class CombineCommand : AsyncCommand<Settings>
{
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
			await AnsiConsole.Status()
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
									AnsiConsole.MarkupLine("[yellow]Changes detected: [/]");
								AnsiConsole.Write(new TextPath(file));
							}
							AnsiConsole.WriteLine();

							if (token.IsCancellationRequested)
								return;

							files = await Merge();
						}
						catch (OperationCanceledException)
						{
							return;
						}
					}
				}));

		Console.ReadKey();
		cancelSource.Cancel();

		return 0;

		async ValueTask<List<FileInfo>> Merge()
		{
			var list = new List<FileInfo>();
			{
				var panel = new PanelBuilder("[white]Files read from:[/]");
				using var output = outputFile.Open(FileMode.Create, FileAccess.Write);
				using var writer = new StreamWriter(output);

				await foreach (var line in rootFile.MergeIncludesAsync(o, info =>
				{
					if (info.FullName == outputFile.FullName)
						throw new InvalidOperationException("Attempting to include the output file.");

					panel.Add(new TextPath(info.FullName));
					list.Add(info);
				}))
				{
					await writer.WriteLineAsync(line);
				}

				await writer.FlushAsync();
				AnsiConsole.Write(panel);
			}

			{
				var mergePath = new TextPath(outputFile.FullName);
				AnsiConsole.Write(new Panel(mergePath)
				{
					Header = new PanelHeader("[springgreen1]Successfully merged include references to:[/]")
				});
			}
			return list;
		}
	}
}
