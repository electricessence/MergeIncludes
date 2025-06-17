using System.Diagnostics;
using System.Text;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit.Abstractions;

namespace MergeIncludes.Tests;

[UsesVerify]
public class ActualOutputTests
{
	private readonly ITestOutputHelper _output;

	public ActualOutputTests(ITestOutputHelper output)
	{
		_output = output;
	}

	private static async Task<string> RunMergeIncludes(string testFile)
	{
		var exePath = Path.Combine("..", "..", "..", "..", "MergeIncludes", "bin", "Debug", "net9.0", "MergeIncludes.exe");
		var fullExePath = Path.GetFullPath(exePath);

		if (!File.Exists(fullExePath))
		{
			throw new FileNotFoundException($"MergeIncludes.exe not found at: {fullExePath}");
		}

		var startInfo = new ProcessStartInfo
		{
			FileName = fullExePath,
			Arguments = $"\"{testFile}\" --display default",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true,
			// Set encoding to UTF-8 for proper character handling
			StandardOutputEncoding = Encoding.UTF8,
			StandardErrorEncoding = Encoding.UTF8
		};

		using var process = Process.Start(startInfo);
		if (process == null)
		{
			throw new InvalidOperationException("Failed to start process");
		}

		var output = await process.StandardOutput.ReadToEndAsync();
		var error = await process.StandardError.ReadToEndAsync();

		await process.WaitForExitAsync();

		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException($"Process failed with exit code {process.ExitCode}. Error: {error}");
		}

		return output;
	}

	[Fact]
	public async Task SimpleConsecutive_ActualOutput()
	{
		var testFile = Path.Combine("TestFiles", "simple-consecutive.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			_output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		// Get the raw application output
		var actualOutput = await RunMergeIncludes(fullPath);

		_output.WriteLine("Actual application output:");
		_output.WriteLine(actualOutput);

		// Process the output through Spectre.Console.Testing.TestConsole
		// This will properly handle ANSI escape sequences
		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("SimpleConsecutive_ActualOutput");
	}

	[Fact]
	public async Task ConsecutiveSameFolder_ActualOutput()
	{
		var testFile = Path.Combine("TestFiles", "consecutive-same-folder-test.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			_output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		// Get the raw application output
		var actualOutput = await RunMergeIncludes(fullPath);

		_output.WriteLine("Actual application output:");
		_output.WriteLine(actualOutput);

		// Process the output through Spectre.Console.Testing.TestConsole
		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("ConsecutiveSameFolder_ActualOutput");
	}

	[Fact]
	public async Task FolderJumping_ActualOutput()
	{
		var testFile = Path.Combine("TestFiles", "unique-names-test.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			_output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		// Get the raw application output
		var actualOutput = await RunMergeIncludes(fullPath);

		_output.WriteLine("Actual application output:");
		_output.WriteLine(actualOutput);

		// Process the output through Spectre.Console.Testing.TestConsole
		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("FolderJumping_ActualOutput");
	}

	[Fact]
	public async Task ComplexCircular_ActualOutput()
	{
		var testFile = Path.Combine("TestFiles", "MainFolder", "complex-root.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			_output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		// Get the raw application output
		var actualOutput = await RunMergeIncludes(fullPath);

		_output.WriteLine("Actual application output:");
		_output.WriteLine(actualOutput);

		// Process the output through Spectre.Console.Testing.TestConsole
		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("ComplexCircular_ActualOutput");
	}
}
