using MergeIncludes.Renderables;
using Spectre.Console.Testing;
using System.Text.RegularExpressions;

namespace MergeIncludes.Tests;

[UsesVerify]
public partial class StructureVerificationTests
{
	private static Dictionary<string, List<string>> BuildFileRelationships(string rootFilePath)
	{
		var fileRelationships = new Dictionary<string, List<string>>();
		var includePattern = IncludePattern();
		var processedFiles = new HashSet<string>();
		var filesToProcess = new Queue<string>();

		filesToProcess.Enqueue(Path.GetFullPath(rootFilePath));

		while (filesToProcess.Count > 0)
		{
			var currentFile = filesToProcess.Dequeue();

			if (processedFiles.Contains(currentFile) || !File.Exists(currentFile))
				continue;

			processedFiles.Add(currentFile);

			try
			{
				var lines = File.ReadAllLines(currentFile);
				var includes = new List<string>();

				foreach (var line in lines)
				{
					var match = includePattern.Match(line);
					if (match.Success)
					{
						var includeFile = match.Groups[1].Value.Trim();
						var fullIncludePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(currentFile)!, includeFile));
						includes.Add(fullIncludePath);
						filesToProcess.Enqueue(fullIncludePath);
					}
				}

				if (includes.Count > 0)
				{
					fileRelationships[currentFile] = includes;
				}
			}
			catch (Exception)
			{
				// Skip files that can't be read
			}
		}

		return fileRelationships;
	}

	[Theory]
	[InlineData("BasicStructure", "simple-root.txt")]
	[InlineData("SimpleConsecutive", "consecutive-same-folder.txt")]
	[InlineData("FolderJumping", "unique-names.txt")]
	[InlineData("DuplicateReferences", "root-duplicates.txt")]
	[InlineData("CircularReferences", "circular-root.txt")]
	[InlineData("ComplexCircular", "complex-root.txt")]
	public async Task VerifyStructureDisplay(string category, string fileName)
	{
		// Arrange
		var testCaseDir = Path.Combine("TestCases", category);
		var rootFilePath = Path.Combine(testCaseDir, fileName);

		if (!File.Exists(rootFilePath))
		{
			throw new FileNotFoundException($"Test file not found: {rootFilePath}");
		}

		var fileRelationships = BuildFileRelationships(rootFilePath);
		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(rootFilePath), fileRelationships);

		// Act
		console.Write(view);
		var output = console.Output;

		// Assert
		await Verify(output)
			.UseParameters(category, Path.GetFileNameWithoutExtension(fileName))
			.UseDirectory("Snapshots/StructureVerification");
	}

	[Fact]
	public async Task VerifyBasicStructure_ExpectedLayout()
	{
		// This test verifies the exact expected structure for basic case
		var testFile = Path.Combine("TestCases", "BasicStructure", "simple-root.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);

		var output = console.Output;

		// The expected structure should be:
		// 📁 TestCases/BasicStructure  / simple-root.txt
		// ├── 📁 SubFolder1             ├── file1.txt [1]
		// ├──                           ├── file2.txt [2]  
		// └── 📁 SubFolder2             └── file3.txt [3]

		await Verify(output)
			.UseDirectory("Snapshots/StructureVerification");
	}

	[Fact]
	public async Task VerifyDuplicateReferences_ShowsAllOccurrences()
	{
		var testFile = Path.Combine("TestCases", "DuplicateReferences", "root-duplicates.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);

		var output = console.Output;

		// Should show:
		// - shared.txt [1] first occurrence in Cyan1
		// - shared.txt [1] second occurrence in gray
		// - All files have unique IDs

		await Verify(output)
			.UseDirectory("Snapshots/StructureVerification");
	}

	[Fact]
	public async Task VerifyCircularReferences_ShowsWarningIcons()
	{
		var testFile = Path.Combine("TestCases", "CircularReferences", "circular-root.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);

		var output = console.Output;

		// Should show warning icons (⚠) for circular references

		await Verify(output)
			.UseDirectory("Snapshots/StructureVerification");
	}

	[Fact]
	public async Task VerifyFolderJumping_RelistsFolders()
	{
		var testFile = Path.Combine("TestCases", "FolderJumping", "unique-names.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);

		var output = console.Output;

		// Should show folders relisted when files jump between them

		await Verify(output)
			.UseDirectory("Snapshots/StructureVerification");
	}

	[Fact]
	public async Task VerifyConsecutiveSameFolder_ShowsAsChildren()
	{
		var testFile = Path.Combine("TestCases", "SimpleConsecutive", "consecutive-same-folder.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);

		var output = console.Output;

		// Consecutive files in same folder should show as children of that folder

		await Verify(output)
			.UseDirectory("Snapshots/StructureVerification");
	}

	[Fact]
	public async Task VerifyComplexStructure_AllFeaturesTogether()
	{
		var testFile = Path.Combine("TestCases", "ComplexCircular", "complex-root.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);

		var output = console.Output;

		// Should demonstrate:
		// - Folder relisting
		// - Duplicate references with proper coloring
		// - Circular reference warnings
		// - Proper alignment between folder and reference trees

		await Verify(output)
			.UseDirectory("Snapshots/StructureVerification");
	}

	[GeneratedRegex(@"^\s*#include\s+(.+)\s*$", RegexOptions.Compiled)]
	private static partial Regex IncludePattern();
}
