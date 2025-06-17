using Spectre.Console.Cli;
using Spectre.Console.Testing;
using Xunit.Abstractions;

namespace MergeIncludes.Tests;

[UsesVerify]
public class ScenarioTests(ITestOutputHelper outputHelper)
{
	private class EmptyRemainingArguments : IRemainingArguments
	{
		private EmptyRemainingArguments()
		{
			// This class is used to provide an empty implementation of IRemainingArguments
			// for testing purposes, as the CombineCommand requires it but we don't need any arguments.
		}

		public ILookup<string, string?> Parsed { get; }
			= Enumerable
				.Empty<KeyValuePair<string, string?>>()
				.ToLookup(x => x.Key, x => x.Value);

		public IReadOnlyList<string> Raw { get; } = [];

		public static readonly EmptyRemainingArguments Instance = new();
	}

	[Theory]
	[InlineData("TestScenarios/01_BasicInclusion/simple-root.txt", "BasicInclusion")]
	[InlineData("TestScenarios/02_DuplicateReferences/root-duplicates.txt", "DuplicateReferences")]
	[InlineData("TestScenarios/03_CircularReferences/circular-root.txt", "CircularReferences")]
	[InlineData("TestScenarios/04_FolderNavigation/unique-names.txt", "FolderNavigation")]
	[InlineData("TestScenarios/05_ConsecutiveIncludes/root-file.txt", "ConsecutiveIncludes")]
	[InlineData("TestScenarios/06_ComplexStructure/complex-root.txt", "ComplexStructure")]
	[InlineData("TestFiles/test-circular.txt", "ManualCircular")]
	public async Task MergeIncludesScenario_OutputTest(string testFilePath, string scenarioName)
	{
		// Arrange
		var fullPath = Path.GetFullPath(testFilePath);

		if (!File.Exists(fullPath))
		{
			outputHelper.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		var testConsole = new TestConsole();
		var settings = new Settings
		{
			RootFilePath = testFilePath,
			DisplayMode = TreeDisplayMode.Default,
			OutputFilePath = ""
		};

		// Act
		var command = new CombineCommand(testConsole);
		var context = new CommandContext(
			[],
			EmptyRemainingArguments.Instance,
			"test-command",
			null);

		await command.ExecuteAsync(context, settings);

		// Assert
		var consoleOutput = testConsole.Output;

		await Verify(consoleOutput)
			.UseDirectory("Snapshots/Scenarios")
			.UseFileName(scenarioName);
	}

	[Theory]
	[InlineData("TestScenarios/01_BasicInclusion/simple-root.txt", TreeDisplayMode.Default, "BasicInclusion_Default")]
	[InlineData("TestScenarios/01_BasicInclusion/simple-root.txt", TreeDisplayMode.FullPath, "BasicInclusion_FullPath")]
	[InlineData("TestScenarios/02_DuplicateReferences/root-duplicates.txt", TreeDisplayMode.Default, "DuplicateReferences_Default")]
	[InlineData("TestScenarios/02_DuplicateReferences/root-duplicates.txt", TreeDisplayMode.FullPath, "DuplicateReferences_FullPath")]
	[InlineData("TestScenarios/06_ComplexStructure/complex-root.txt", TreeDisplayMode.Default, "ComplexStructure_Default")]
	public async Task MergeIncludesDisplayMode_OutputTest(string testFilePath, TreeDisplayMode displayMode, string testName)
	{
		// Arrange
		var fullPath = Path.GetFullPath(testFilePath);

		if (!File.Exists(fullPath))
		{
			outputHelper.WriteLine($"Test file not found: {fullPath}");
			return;
		}

		var testConsole = new TestConsole();
		var settings = new Settings
		{
			RootFilePath = testFilePath,
			DisplayMode = displayMode,
			OutputFilePath = ""
		};

		// Act
		var command = new CombineCommand(testConsole);
		var context = new CommandContext(
			[],
			EmptyRemainingArguments.Instance,
			"test-command",
			null);

		await command.ExecuteAsync(context, settings);

		// Assert
		var consoleOutput = testConsole.Output;

		await Verify(consoleOutput)
			.UseDirectory("Snapshots/DisplayModes")
			.UseFileName(testName);
	}
}
