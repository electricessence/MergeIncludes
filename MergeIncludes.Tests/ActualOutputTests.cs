using Spectre.Console.Testing;
using Xunit.Abstractions;

namespace MergeIncludes.Tests;

[UsesVerify]
public class ActualOutputTests(ITestOutputHelper output)
{
	/// <summary>
	/// Test a merge scenario and verify both merged content and console output separately
	/// </summary>
	private async Task TestMergeScenario(string testFilePath, string scenarioName)
	{
		var settings = new Settings
		{
			RootFilePath = testFilePath,
			DisplayMode = TreeDisplayMode.Default,
			Trim = true
		};

		output.WriteLine($"Testing scenario: {scenarioName}");
		output.WriteLine($"File: {testFilePath}");

		// Get the merge result without writing files
		var result = await CombineCommand.MergeToMemoryAsync(settings);

		// Verify the merged content separately
		await Verify(result.MergedContent ?? result.ErrorMessage ?? "No content")
			.UseDirectory("Snapshots/MergedContent")
			.UseFileName($"{scenarioName}_MergedContent");

		// For successful merges, also verify the console display output
		if (result.IsSuccess && result.MergedContent != null)
		{
			var testConsole = new TestConsole();

			// Simulate the tree display that would be shown to users
			var rootFile = new FileInfo(testFilePath);
			var structureView = new Renderables.StructureAndReferenceView(rootFile, result.FileRelationships);
			testConsole.Write(structureView);

			var consoleOutput = testConsole.Output;

			// Verify the console output separately
			await Verify(consoleOutput)
				.UseDirectory("Snapshots/ConsoleOutput")
				.UseFileName($"{scenarioName}_ConsoleOutput");
		}
		else
		{
			// For error cases, verify the error handling display
			await Verify(new
			{
				Success = result.IsSuccess,
				result.ErrorMessage,
				ProcessedFiles = result.ProcessedFiles.Count
			})
				.UseDirectory("Snapshots/ConsoleOutput")
				.UseFileName($"{scenarioName}_ErrorOutput");
		}
	}
	// Test scenarios using organized TestScenarios directory

	[Fact]
	public async Task SimpleConsecutive_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "05_ConsecutiveIncludes", "simple-consecutive.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "SimpleConsecutive");
	}

	[Fact]
	public async Task ConsecutiveSameFolder_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "05_ConsecutiveIncludes", "consecutive-same-folder.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "ConsecutiveSameFolder");
	}

	[Fact]
	public async Task FolderJumping_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "04_FolderNavigation", "root.txt");
		var fullPath = Path.GetFullPath(testFile);
		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "FolderJumping");
	}

	[Fact]
	public async Task ComplexCircular_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "Shared", "MainFolder", "complex-root.txt");
		var fullPath = Path.GetFullPath(testFile);
		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "ComplexCircular");
	}

	[Fact]
	public async Task BasicStructure_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "01_BasicInclusion", "root.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "BasicStructure");
	}

	[Fact]
	public async Task DuplicateReferences_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "02_DuplicateReferences", "root.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "DuplicateReferences");
	}

	[Fact]
	public async Task OrganizedCircularReference_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "03_CircularReferences", "root.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "OrganizedCircularReference");
	}

	[Fact]
	public async Task OrganizedFolderJumping_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "04_FolderNavigation", "root.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "OrganizedFolderJumping");
	}

	[Fact]
	public async Task RootFile_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "Shared", "MainFolder", "root.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "RootFile");
	}

	[Fact]
	public async Task TestDuplicates_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "02_DuplicateReferences", "test-duplicates.txt");
		var fullPath = Path.GetFullPath(testFile);

		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "TestDuplicates");
	}

	[Fact]
	public async Task FolderJumpingTest_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "10_DetailedFolderJumping", "root.txt");
		var fullPath = Path.GetFullPath(testFile);
		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "FolderJumpingTest");
	}

	[Fact]
	public async Task SimpleRootFile_ActualOutput()
	{
		var testFile = Path.Combine("TestScenarios", "Shared", "root-file.txt");
		var fullPath = Path.GetFullPath(testFile);
		if (!File.Exists(fullPath))
		{
			output.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		await TestMergeScenario(fullPath, "SimpleRootFile");
	}
}
