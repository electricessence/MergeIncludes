using MergeIncludes;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<CombineCommand>();

app.Configure(config =>
{
	config.ValidateExamples();
	config.SetExceptionHandler((ex, resolver) =>
	{
		//AnsiConsole.WriteException(ex);
		switch (ex)
		{
			case CommandRuntimeException cex:
				AnsiConsole.Write(new Markup("[red]Error: [/]"));
				AnsiConsole.WriteLine(cex.Message);
				return 1;

			case FileNotFoundException fnfex:
				fnfex.WriteToConsole("File Not Found");
				return 1;

			default:
				AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
				return -99;
		}
	});
});

// Capture the exit code and propagate it to the host
int exitCode = await app.RunAsync(args);
return exitCode;
