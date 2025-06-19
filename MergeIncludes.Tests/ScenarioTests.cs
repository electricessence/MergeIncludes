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
	[InlineData("TestScenarios/01_BasicInclusion/root.txt", "BasicInclusion")]
	[InlineData("TestScenarios/02_DuplicateReferences/root.txt", "DuplicateReferences")]
	[InlineData("TestScenarios/03_CircularReferences/root.txt", "CircularReferences")]
	[InlineData("TestScenarios/04_FolderNavigation/root.txt", "FolderNavigation")]
	[InlineData("TestScenarios/05_ConsecutiveIncludes/root.txt", "ConsecutiveIncludes")]
	[InlineData("TestScenarios/05_EscapeSequences/root.txt", "EscapeSequences")]
	[InlineData("TestScenarios/06_ComplexStructure/root.txt", "ComplexStructure")]
	[InlineData("TestScenarios/07_WildcardIncludes/root.txt", "WildcardIncludes")]
	[InlineData("TestScenarios/08_AlternatingFolders/root.txt", "AlternatingFolders")]
	[InlineData("TestScenarios/08_CommentedIncludes/root.txt", "CommentedIncludes")]
	[InlineData("TestScenarios/09_UniqueFilenames/root.txt", "UniqueFilenames")]
	[InlineData("TestScenarios/10_DetailedFolderJumping/root.txt", "DetailedFolderJumping")]
	[InlineData("TestScenarios/11_NestedRequireTree/types-modular.pine", "NestedRequireTree")]
	[InlineData("TestScenarios/wildcard-reference/main.txt", "WildcardReference")]
	[InlineData("TestScenarios/Shared/root-file.txt", "Shared")]
	[InlineData("TestScenarios/03_CircularReferences/manual-circular.txt", "ManualCircular")]
	[InlineData("TestScenarios/12_FileNotFound/root.txt", "FileNotFound")]
	public async Task ScenarioTest(string testFilePath, string scenarioName)
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

		// Act - Test console output
		var command = new CombineCommand(testConsole);
		var context = new CommandContext(
			[],
			EmptyRemainingArguments.Instance,
			"test-command",
			null);
		await command.ExecuteAsync(context, settings);

		// Assert - Verify console output
		var consoleOutput = testConsole.Output;
		await Verify(consoleOutput)
			.UseDirectory("Snapshots")
			.UseFileName(scenarioName);

		// Act - Test merged content
		var mergeResult = await CombineCommand.MergeToMemoryAsync(settings);

		// Assert - Verify merged content
		await Verify(mergeResult.MergedContent ?? mergeResult.ErrorMessage ?? "No content")
			.UseDirectory("Snapshots")
			.UseFileName($"{scenarioName}.MergedContent");
	}
}