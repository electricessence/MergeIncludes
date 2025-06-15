using MergeIncludes;
using Spectre.Console;
using Spectre.Console.Cli;

Console.OutputEncoding = System.Text.Encoding.UTF8;

try
{
	// Use a simpler approach without custom type registration
	var app = new CommandApp<CombineCommand>();
	app.Configure(config =>
	{
		config.ValidateExamples();
		config.SetExceptionHandler((ex, _) =>
		{
			// Use the default console
			var console = AnsiConsole.Console;

			switch (ex)
			{
				case CommandRuntimeException cex:
					console.Write(new Markup("[red]Error: [/]"));
					console.WriteLine(cex.Message);
					return 1;

				case FileNotFoundException fnfex:
					fnfex.WriteToConsole("File Not Found", console);
					return 1;

				default:
					console.WriteException(ex, ExceptionFormats.ShortenEverything);
					return -99;
			}
		});
	});

	// Capture the exit code and propagate it to the host
	int exitCode = await app.RunAsync(args);
	return exitCode;
}
catch (Exception ex)
{
	// Fallback exception handler
	AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
	return -1;
}
