using MergeIncludes;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<CombineCommand>();

app.Configure(config =>
{
	config.ValidateExamples();
	config.SetExceptionHandler(ex =>
	{
		switch (ex)
		{
			case CommandRuntimeException cex:
				AnsiConsole.Write(new Markup("[red]Error: [/]"));
				AnsiConsole.WriteLine(cex.Message);
				return 1;

			default:
				AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
				return -99;
		}
	});
});

await app.RunAsync(args);
