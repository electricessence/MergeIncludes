namespace MergeIncludes.Tests;

/// <summary>
/// Tests for the new MergeToMemoryAsync method
/// </summary>
public class MergeToMemoryTests
{
	[Fact]
	public async Task MergeToMemoryAsync_BasicInclusion_ReturnsSuccessfulResult()
	{
		// Arrange
		var testScenarioPath = Path.Combine(
			Directory.GetCurrentDirectory(),
			"TestScenarios",
			"01_BasicInclusion",
			"root.txt");

		var settings = new Settings
		{
			RootFilePath = testScenarioPath,
			Trim = true
		};

		// Act
		var result = await CombineCommand.MergeToMemoryAsync(settings);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.MergedContent);
		Assert.NotEmpty(result.MergedContent);
		Assert.Null(result.ErrorMessage);
		Assert.NotEmpty(result.ProcessedFiles);
		Assert.NotEmpty(result.FileRelationships);
		// Verify the content contains the expected merged text
		Assert.Contains("This is a basic test with simple includes", result.MergedContent);
		Assert.Contains("This is file1 in SubFolder1", result.MergedContent);
		Assert.Contains("This is file2 in SubFolder2", result.MergedContent);
		Assert.Contains("This is file3 in SubFolder1", result.MergedContent);

		// Verify that include statements are not in the merged content
		Assert.DoesNotContain("#include", result.MergedContent);
	}

	[Fact]
	public async Task MergeToMemoryAsync_CircularReference_ReturnsFailureResult()
	{
		// Arrange
		var testScenarioPath = Path.Combine(
			Directory.GetCurrentDirectory(),
			"TestScenarios",
			"03_CircularReferences",
			"root.txt");

		var settings = new Settings
		{
			RootFilePath = testScenarioPath,
			Trim = true
		};

		// Act
		var result = await CombineCommand.MergeToMemoryAsync(settings);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Null(result.MergedContent);
		Assert.NotNull(result.ErrorMessage);
		Assert.NotEmpty(result.ErrorMessage);
		Assert.Contains("Detected recursive reference", result.ErrorMessage);
		Assert.NotEmpty(result.ProcessedFiles);
		Assert.NotEmpty(result.FileRelationships);
	}

	[Theory]
	[InlineData("01_BasicInclusion", true)]
	[InlineData("02_DuplicateReferences", true)]
	[InlineData("03_CircularReferences", false)]
	[InlineData("04_FolderNavigation", true)]
	[InlineData("05_ConsecutiveIncludes", true)]
	[InlineData("06_ComplexStructure", true)]
	public async Task MergeToMemoryAsync_AllScenarios_ProducesExpectedResults(string scenario, bool shouldSucceed)
	{
		// Arrange
		var testScenarioPath = Path.Combine(
			Directory.GetCurrentDirectory(),
			"TestScenarios",
			scenario,
			"root.txt");

		// Skip test if file doesn't exist (scenario not yet implemented)
		if (!File.Exists(testScenarioPath))
		{
			return;
		}

		var settings = new Settings
		{
			RootFilePath = testScenarioPath,
			Trim = true
		};

		// Act
		var result = await CombineCommand.MergeToMemoryAsync(settings);

		// Assert
		Assert.Equal(shouldSucceed, result.IsSuccess);

		if (shouldSucceed)
		{
			Assert.NotNull(result.MergedContent);
			Assert.NotEmpty(result.MergedContent);
			Assert.Null(result.ErrorMessage);
		}
		else
		{
			Assert.Null(result.MergedContent);
			Assert.NotNull(result.ErrorMessage);
			Assert.NotEmpty(result.ErrorMessage);
		}

		Assert.NotEmpty(result.ProcessedFiles);
		Assert.NotEmpty(result.FileRelationships);
	}
}
