using MergeIncludes;
using Spectre.Console;
using Spectre.Console.Cli;

// Create a command app with the default AnsiConsole
var app = new CommandApp<CombineCommand>(new TypeRegistrar(AnsiConsole.Console));

app.Configure(config =>
{
	config.ValidateExamples();
	config.SetExceptionHandler((ex, resolver) =>
	{
		// Get the console from the resolver if available, or use the default
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

/// <summary>
/// A simple type registrar for Spectre.Console.Cli to inject IAnsiConsole
/// </summary>
public class TypeRegistrar : ITypeRegistrar
{
	private readonly IAnsiConsole _console;
	private readonly Dictionary<Type, object> _registrations = new();

	public TypeRegistrar(IAnsiConsole console)
	{
		_console = console ?? throw new ArgumentNullException(nameof(console));
	}

	public ITypeResolver Build()
	{
		return new TypeResolver(_registrations);
	}

	public void Register(Type service, Type implementation)
	{
		if (service == typeof(IAnsiConsole))
		{
			_registrations[service] = _console;
		}
	}

	public void RegisterInstance(Type service, object implementation)
	{
		_registrations[service] = implementation;
	}

	public void RegisterLazy(Type service, Func<object> func)
	{
		if (service == typeof(IAnsiConsole))
		{
			_registrations[service] = _console;
		}
		else
		{
			_registrations[service] = func();
		}
	}
}

/// <summary>
/// Type resolver for Spectre.Console.Cli
/// </summary>
public class TypeResolver : ITypeResolver
{
	private readonly Dictionary<Type, object> _registrations;

	public TypeResolver(Dictionary<Type, object> registrations)
	{
		_registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
	}

	public object? Resolve(Type? type)
	{
		if (type == null)
		{
			return null;
		}

		if (_registrations.TryGetValue(type, out var value))
		{
			return value;
		}

		return null;
	}
}
