using Spectre.Console.Testing;
using System.Reflection;

namespace MergeIncludes.Tests;

[UsesVerify]
public class FolderStructureTests
{
	[Fact]
	public async Task ComplexFolderStructure_DisplaysCorrectly()
	{
		// Arrange
		var console = new TestConsole();
		var rootFile = new FileInfo(Path.GetFullPath(@".\TestFiles\MainFolder\root.txt"));
		var fileRelationships = BuildComplexFileRelationships();

		// Act - Use the StructureAndReferenceView directly
		var structureAndReferenceView = new Renderables.StructureAndReferenceView(rootFile, fileRelationships);
		console.Write(structureAndReferenceView);

		// Assert - Verify console output
		var output = console.Output;
		await Verify(output)
			.UseDirectory("Snapshots")
			.UseFileName("ComplexFolderStructure");
	}

	[Fact]
	public async Task ComplexFolderStructure_DetectsRepeatedDependencies()
	{
		// Arrange
		var console = new TestConsole();
		var rootFile = new FileInfo(Path.GetFullPath(@".\TestFiles\MainFolder\root.txt"));
		var fileRelationships = BuildComplexFileRelationships();

		// Act - Use the StructureAndReferenceView directly for repeated dependencies
		var structureAndReferenceView = new Renderables.StructureAndReferenceView(rootFile, fileRelationships);
		console.Write(structureAndReferenceView);

		// Assert - Verify console output
		var output = console.Output;
		await Verify(output)
			.UseDirectory("Snapshots")
			.UseFileName("ComplexFolderStructureWithRepeats");
	}

	private Dictionary<string, List<string>> BuildComplexFileRelationships()
	{
		var rootPath = Path.GetFullPath(@".\TestFiles\MainFolder\root.txt");
		var basePath = Path.GetFullPath(@".\TestFiles");

		var mainFolder = Path.Combine(basePath, "MainFolder");
		var subFolder1 = Path.Combine(mainFolder, "SubFolder1");
		var subFolder2 = Path.Combine(mainFolder, "SubFolder2");
		var anotherFolder = Path.Combine(basePath, "AnotherFolder");

		// Define all file paths
		var localPath = Path.Combine(mainFolder, "local.txt");
		var component1Path = Path.Combine(subFolder1, "component1.txt");
		var subcomponent1Path = Path.Combine(subFolder1, "subcomponent1.txt");
		var component2Path = Path.Combine(subFolder2, "component2.txt");
		var commonPath = Path.Combine(subFolder2, "common.txt");
		var externalPath = Path.Combine(anotherFolder, "external.txt");

		var fileRelationships = new Dictionary<string, List<string>>
		{
			// Root file includes
			[rootPath] =
		[
			localPath,
			component1Path,
			component2Path,
			externalPath
		],

			// Component1 includes
			[component1Path] =
		[
			subcomponent1Path,
			localPath,          // This is a repeat dependency since it's also included by root
            commonPath
		],

			// Component2 includes
			[component2Path] =
		[
			commonPath,
			subcomponent1Path   // This will be a repeat when component1 is also included
        ],

			// External file includes
			[externalPath] =
		[
			commonPath          // This will be a repeat across folder boundaries
        ]
		};

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
