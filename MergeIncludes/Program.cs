using CommandLine;
using MergeIncludes;
using Spectre.Console;
using Spectre.Console.Extensions;
using Throw;

await Parser.Default
	.ParseArguments<Options>(args)
	.WithParsedAsync(async o =>
	{
		o.ThrowIfNull().OnlyInDebug();
		var rootFile = o.GetRootFile();

		if (!rootFile.Exists)
		{
			AnsiConsole.MarkupLine("[red]Root file not found:[/]");
			var path = new TextPath(rootFile.FullName);
			AnsiConsole.Write(path);
			return;
		}

		var outputFile = o.GetOutputFile(rootFile);

		var files = await Merge();

		if (!o.Watch) return;

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

		async ValueTask<List<FileInfo>> Merge()
		{
			var list = new List<FileInfo>();
			{
				var panel = new PanelBuilder("[white]Files read from:[/]");
				using var output = outputFile.Open(FileMode.Create, FileAccess.Write);
				using var writer = new StreamWriter(output);

				await foreach (var line in rootFile.MergeIncludesAsync(info =>
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
	});
