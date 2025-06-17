using Spectre.Console;
using Spectre.Console.Testing;
using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace MergeIncludes.Tests;

[UsesVerify]
public class ActualOutputTests(ITestOutputHelper output)
{
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

		using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start process");
		var output = await process.StandardOutput.ReadToEndAsync();
		var error = await process.StandardError.ReadToEndAsync();

		await process.WaitForExitAsync();

		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException($"Process failed with exit code {process.ExitCode}. Error: {error}");
		}

		return output;
	}

	// Legacy tests using TestFiles directory
	
	[Fact]
	public async Task SimpleConsecutive_ActualOutput()
	{
		var testFile = Path.Combine("TestFiles", "simple-consecutive.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			// Attempt to use the migrated file in TestCases if available
			var altPath = Path.GetFullPath(Path.Combine("TestCases", "SimpleConsecutive", "consecutive-same-folder.txt"));
			if (File.Exists(altPath))
			{
				output.WriteLine($"Using alternative file: {altPath}");
				fullPath = altPath;
			}
			else
			{
				return;
			}
		}

		// Get the raw application output
		var actualOutput = await RunMergeIncludes(fullPath);

		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

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
			output.WriteLine($"Test file not found: {fullPath}");
			// Attempt to use the migrated file in TestCases if available
			var altPath = Path.GetFullPath(Path.Combine("TestCases", "SimpleConsecutive", "consecutive-same-folder.txt"));
			if (File.Exists(altPath))
			{
				output.WriteLine($"Using alternative file: {altPath}");
				fullPath = altPath;
			}
			else
			{
				return;
			}
		}

		// Get the raw application output
		var actualOutput = await RunMergeIncludes(fullPath);

		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

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
			output.WriteLine($"Test file not found: {fullPath}");
			// Attempt to use the migrated file in TestCases if available
			var altPath = Path.GetFullPath(Path.Combine("TestCases", "FolderJumping", "unique-names.txt"));
			if (File.Exists(altPath))
			{
				output.WriteLine($"Using alternative file: {altPath}");
				fullPath = altPath;
			}
			else
			{
				return;
			}
		}

		// Get the raw application output
		var actualOutput = await RunMergeIncludes(fullPath);

		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

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
			output.WriteLine($"Test file not found: {fullPath}");
			// Attempt to use the migrated file in TestCases if available
			var altPath = Path.GetFullPath(Path.Combine("TestCases", "ComplexCircular", "complex-root.txt"));
			if (File.Exists(altPath))
			{
				output.WriteLine($"Using alternative file: {altPath}");
				fullPath = altPath;
			}
			else
			{
				return;
			}
		}

		// Get the raw application output
		var actualOutput = await RunMergeIncludes(fullPath);

		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

		// Process the output through Spectre.Console.Testing.TestConsole
		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("ComplexCircular_ActualOutput");
	}
	
	// New organized tests using TestCases directory
	
	[Fact]
	public async Task BasicStructure_ActualOutput()
	{
		var testFile = Path.Combine("TestCases", "BasicStructure", "simple-root.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		var actualOutput = await RunMergeIncludes(fullPath);
		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("BasicStructure_ActualOutput");
	}

	[Fact]
	public async Task DuplicateReferences_ActualOutput()
	{
		var testFile = Path.Combine("TestCases", "DuplicateReferences", "root-duplicates.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		var actualOutput = await RunMergeIncludes(fullPath);
		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("DuplicateReferences_ActualOutput");
	}

	[Fact]
	public async Task OrganizedCircularReference_ActualOutput()
	{
		var testFile = Path.Combine("TestCases", "CircularReferences", "circular-root.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		var actualOutput = await RunMergeIncludes(fullPath);
		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("OrganizedCircularReference_ActualOutput");
	}

	[Fact]
	public async Task OrganizedFolderJumping_ActualOutput()
	{
		var testFile = Path.Combine("TestCases", "FolderJumping", "unique-names.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		var actualOutput = await RunMergeIncludes(fullPath);
		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("OrganizedFolderJumping_ActualOutput");
	}

	// Remaining legacy tests
	
	[Fact]
	public async Task RootFile_ActualOutput()
	{
		var testFile = Path.Combine("TestFiles", "MainFolder", "root.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		var actualOutput = await RunMergeIncludes(fullPath);
		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("RootFile_ActualOutput");
	}

	[Fact]
	public async Task TestDuplicates_ActualOutput()
	{
		var testFile = Path.Combine("TestFiles", "test-duplicates.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			// Try alternative path in TestCases
			var altPath = Path.GetFullPath(Path.Combine("TestCases", "DuplicateReferences", "root-duplicates.txt"));
			if (File.Exists(altPath))
			{
				output.WriteLine($"Using alternative file: {altPath}");
				fullPath = altPath;
			}
			else
			{
				return;
			}
		}

		var actualOutput = await RunMergeIncludes(fullPath);
		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("TestDuplicates_ActualOutput");
	}

	[Fact]
	public async Task FolderJumpingTest_ActualOutput()
	{
		var testFile = Path.Combine("TestFiles", "folder-jumping-test.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			// Try alternative path in TestCases
			var altPath = Path.GetFullPath(Path.Combine("TestCases", "FolderJumping", "unique-names.txt"));
			if (File.Exists(altPath))
			{
				output.WriteLine($"Using alternative file: {altPath}");
				fullPath = altPath;
			}
			else
			{
				return;
			}
		}

		var actualOutput = await RunMergeIncludes(fullPath);
		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("FolderJumpingTest_ActualOutput");
	}

	[Fact]
	public async Task SimpleRootFile_ActualOutput()
	{
		var testFile = Path.Combine("TestFiles", "root-file.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			// Try alternative path in TestCases
			var altPath = Path.GetFullPath(Path.Combine("TestCases", "BasicStructure", "simple-root.txt"));
			if (File.Exists(altPath))
			{
				output.WriteLine($"Using alternative file: {altPath}");
				fullPath = altPath;
			}
			else
			{
				return;
			}
		}

		var actualOutput = await RunMergeIncludes(fullPath);
		output.WriteLine("Actual application output:");
		output.WriteLine(actualOutput);

		var testConsole = new TestConsole();
		testConsole.Write(new Markup(actualOutput));
		var processedOutput = testConsole.Output;

		await Verify(processedOutput)
			 .UseDirectory("Snapshots/ActualOutput")
			 .UseFileName("SimpleRootFile_ActualOutput");
	}
}
