using MergeIncludes.TreeBuilders;
using Spectre.Console.Testing;

namespace MergeIncludes.Tests;

[UsesVerify]
public class SimpleReferenceTreeBuilderTests
{
	[Fact]
	public async Task SingleFile_NoChildren_ShowsOnlyRoot()
	{
		// Arrange
		var rootPath = "root.txt";
		var relationships = new Dictionary<string, List<string>>();

		// Act
		var tree = SimpleReferenceTreeBuilder.BuildReferenceTree(rootPath, relationships);

		// Assert
		var console = new TestConsole();
		console.Write(tree);
		var output = console.Output;
		await Verify(output)
		  .UseDirectory("Snapshots")
		  .UseMethodName("SingleFile_NoChildren");
	}

	[Fact]
	public async Task RootWithOneChild_ShowsCorrectHierarchy()
	{
		// Arrange
		var rootPath = "root.txt";
		var relationships = new Dictionary<string, List<string>>
		{
			[rootPath] = ["child.txt"]
		};

		// Act
		var tree = SimpleReferenceTreeBuilder.BuildReferenceTree(rootPath, relationships);

		// Assert
		var console = new TestConsole();
		console.Write(tree);
		var output = console.Output; await Verify(output)
			.UseDirectory("Snapshots")
			.UseMethodName("RootWithOneChild");
	}

	[Fact]
	public async Task RootWithMultipleChildren_ShowsAllChildren()
	{
		// Arrange
		var rootPath = "root.txt";
		var relationships = new Dictionary<string, List<string>>
		{
			[rootPath] = ["child1.txt", "child2.txt", "child3.txt"]
		};

		// Act
		var tree = SimpleReferenceTreeBuilder.BuildReferenceTree(rootPath, relationships);

		// Assert
		var console = new TestConsole();
		console.Write(tree);
		var output = console.Output; await Verify(output)
			.UseDirectory("Snapshots")
			.UseMethodName("RootWithMultipleChildren");
	}

	[Fact]
	public async Task NestedHierarchy_ShowsCorrectIndentation()
	{
		// Arrange - This matches the NestedRequireTree scenario structure
		var rootPath = "types-modular.pine";
		var coreTypesPath = "0200 - core-types.pine";
		var relationships = new Dictionary<string, List<string>>
		{
			[rootPath] = ["0100 - constants.pine", coreTypesPath, "0300 - factories.pine"],
			[coreTypesPath] = [
				"SignalThought.partial.pine",
				"SignalDefinition.partial.pine",
				"SignalConfiguration.partial.pine",
				"SignalInstance.partial.pine",
				"SignalGroup.partial.pine",
				"SignalRegistry.partial.pine",
				"SignalsFramework.partial.pine"
			]
		};

		// Act
		var tree = SimpleReferenceTreeBuilder.BuildReferenceTree(rootPath, relationships);

		// Assert
		var console = new TestConsole();
		console.Write(tree);
		var output = console.Output; await Verify(output)
			.UseDirectory("Snapshots")
			.UseMethodName("NestedHierarchy");
	}

	[Fact]
	public async Task ComplexStructure_MatchesExpectedPattern()
	{
		// Arrange - This matches the ComplexFolderStructure scenario
		var rootPath = "complex-root.txt";
		var component1Path = "component1.txt";
		var component2Path = "component2.txt";
		var relationships = new Dictionary<string, List<string>>
		{
			[rootPath] = ["local.txt", component1Path, component2Path, "common.txt", "subcomponent1.txt", "external.txt"],
			[component1Path] = ["subcomponent1.txt", "local.txt", "common.txt"],
			[component2Path] = ["common.txt", "subcomponent1.txt"]
		};

		// Act
		var tree = SimpleReferenceTreeBuilder.BuildReferenceTree(rootPath, relationships);

		// Assert
		var console = new TestConsole();
		console.Write(tree);
		var output = console.Output; await Verify(output)
			.UseDirectory("Snapshots")
			.UseMethodName("ComplexStructure");
	}

	[Fact]
	public async Task CircularReference_DoesNotCauseInfiniteLoop()
	{
		// Arrange
		var rootPath = "root.txt";
		var childPath = "child.txt";
		var relationships = new Dictionary<string, List<string>>
		{
			[rootPath] = [childPath],
			[childPath] = [rootPath] // Circular reference
		};

		// Act
		var tree = SimpleReferenceTreeBuilder.BuildReferenceTree(rootPath, relationships);

		// Assert
		var console = new TestConsole();
		console.Write(tree);
		var output = console.Output; await Verify(output)
			.UseDirectory("Snapshots")
			.UseMethodName("CircularReference");
	}

	[Fact]
	public async Task DuplicateFiles_ShowNumberedIdentifiers()
	{
		// Arrange - A file that appears multiple times in different parts of the tree
		var rootPath = "root.txt";
		var sharedPath = "shared.txt";
		var relationships = new Dictionary<string, List<string>>
		{
			[rootPath] = ["component1.txt", "component2.txt", sharedPath], // shared.txt appears first time
			["component1.txt"] = [sharedPath, "local1.txt"], // shared.txt appears second time
			["component2.txt"] = [sharedPath, "local2.txt"]  // shared.txt appears third time
		};

		// Act
		var tree = SimpleReferenceTreeBuilder.BuildReferenceTree(rootPath, relationships);

		// Assert
		var console = new TestConsole();
		console.Write(tree);
		var output = console.Output;

		await Verify(output)
			.UseDirectory("Snapshots")
			.UseMethodName("DuplicateFiles_ShowNumberedIdentifiers");
	}

	[Fact]
	public async Task MixedUniqueAndDuplicateFiles_ShowCorrectColorsAndIds()
	{
		// Arrange - Mix of unique files and duplicates to test color coding
		var rootPath = "main.txt";
		var sharedPath = "shared.txt";
		var uniquePath = "unique.txt";
		var relationships = new Dictionary<string, List<string>>
		{
			[rootPath] = [uniquePath, "component1.txt", "component2.txt", sharedPath],
			["component1.txt"] = [sharedPath, "component1-specific.txt"], // shared.txt appears again
			["component2.txt"] = ["component2-specific.txt"] // no shared files
		};

		// Act
		var tree = SimpleReferenceTreeBuilder.BuildReferenceTree(rootPath, relationships);

		// Assert
		var console = new TestConsole();
		console.Write(tree);
		var output = console.Output;

		await Verify(output)
			.UseDirectory("Snapshots")
			.UseMethodName("MixedUniqueAndDuplicateFiles_ShowCorrectColorsAndIds");
	}
	[Fact]
	public async Task ReproduceSignalFrameworkDuplicateIssue_ShouldNumberAllDuplicates()
	{
		// Arrange - This reproduces the exact scenario from the signal framework where 
		// one file gets [1] but another file doesn't get numbered
		var rootPath = "01.txt";
		var relationships = new Dictionary<string, List<string>>
		{
			[rootPath] = ["02.txt", "03.txt", "04.txt", "05.txt", "06.txt", "07.txt"],
			["04.txt"] = ["08.txt"],
			["08.txt"] = ["09.txt"],
			["09.txt"] = ["10.txt", "11.txt", "12.txt"],
			["10.txt"] = ["13.txt", "14.txt"],
			["11.txt"] = ["13.txt"], // Same file as above - should be [1]
			["13.txt"] = ["15.txt"], // This appears in both paths above
			["12.txt"] = ["13.txt"], // Same file again - should still be [1]
									 // Note: 15.txt appears multiple times via different 13.txt references
		};

		// Act
		var tree = SimpleReferenceTreeBuilder.BuildReferenceTree(rootPath, relationships);

		// Assert
		var console = new TestConsole();
		console.Write(tree);
		var output = console.Output;

		// Both 13.txt AND 15.txt should be numbered
		// since they both appear multiple times in the tree
		await Verify(output)
			.UseDirectory("Snapshots")
			.UseMethodName("ReproduceSignalFrameworkDuplicateIssue_ShouldNumberAllDuplicates");
	}
}
