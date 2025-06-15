using Spectre.Console.Testing;
using System.Reflection;

namespace MergeIncludes.Tests;

[UsesVerify]
public class CombineCommandTests
{
	[Fact]
	public async Task DefaultTreeDisplayMode_ShowsCorrectStructure()
	{
		// Arrange
		var console = new TestConsole();
		var rootFile = new FileInfo(Path.GetFullPath(@".\sample.txt"));
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
	private Dictionary<string, List<string>> CreateTestFileRelationships(FileInfo rootFile)
	{
		var fileRelationships = new Dictionary<string, List<string>>
		{
			// Add sample01.txt and sample02.txt as children of sample.txt for testing
			[rootFile.FullName] =
		[
			Path.GetFullPath(@".\sample01.txt"),
			Path.GetFullPath(@".\sample02.txt")
		]
		};

		// Add sample02-01.txt as a child of sample02.txt for testing
		var sample02Path = Path.GetFullPath(@".\sample02.txt");
		fileRelationships[sample02Path] =
		[
			Path.GetFullPath(@".\sample02-01.txt")
		];

		return fileRelationships;
	}

	// Helper method to invoke the private DisplayFileTrees method using reflection
	private void InvokeDisplayFileTrees(
		CombineCommand command,
		FileInfo rootFile,
		Dictionary<string, List<string>> fileRelationships,
		TreeDisplayMode mode)
	{
		var methodInfo = typeof(CombineCommand).GetMethod(
			"DisplayFileTrees",
			BindingFlags.NonPublic | BindingFlags.Instance);

		if (methodInfo == null)
		{
			throw new InvalidOperationException("Could not find the DisplayFileTrees method");
		}

		methodInfo.Invoke(command, new object[] { rootFile, fileRelationships, mode });
	}
}
