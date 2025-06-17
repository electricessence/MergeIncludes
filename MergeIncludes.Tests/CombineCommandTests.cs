using Spectre.Console.Testing;

namespace MergeIncludes.Tests;

[UsesVerify]
public class CombineCommandTests
{
	[Fact]
	public async Task DefaultTreeDisplayMode_ShowsCorrectStructure()
	{
		// Arrange
		var console = new TestConsole(); var rootFile = new FileInfo(Path.GetFullPath(@"TestScenarios\08_CommentedIncludes\root.txt"));
		var fileRelationships = CreateTestFileRelationships(rootFile);

		// Act - Use the StructureAndReferenceView directly
		var structureAndReferenceView = new Renderables.StructureAndReferenceView(rootFile, fileRelationships);
		console.Write(structureAndReferenceView);

		// Assert - Verify console output
		var output = console.Output;
		await Verify(output)
			.UseDirectory("Snapshots")
			.UseFileName("DefaultTreeDisplay");
	}
	// Helper method to create a test file relationship dictionary
	private static Dictionary<string, List<string>> CreateTestFileRelationships(FileInfo rootFile)
	{
		var fileRelationships = new Dictionary<string, List<string>>
		{
			// Add sample01.txt from wildcard includes as children of root for testing
			[rootFile.FullName] =
		[
			Path.GetFullPath(@"TestScenarios\07_WildcardIncludes\sample01.txt"),
			Path.GetFullPath(@"TestScenarios\07_WildcardIncludes\sample02.txt")
		]
		};

		// Add sample02-01.txt as a child of sample02.txt for testing
		var sample02Path = Path.GetFullPath(@"TestScenarios\07_WildcardIncludes\sample02.txt");
		fileRelationships[sample02Path] =
		[
			Path.GetFullPath(@"TestScenarios\07_WildcardIncludes\sample02-01.txt")
		];

		return fileRelationships;
	}
}
