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

	[Fact]	public async Task VerifyBasicStructure_ExpectedLayout()
	{
		// Test verifies the exact expected structure for basic case
		var testFile = Path.Combine("TestScenarios", "01_BasicInclusion", "root.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);
		var output = console.Output;

		// Trim trailing whitespace from each line to avoid test failures
		var trimmedOutput = TrimTrailingWhitespace(output);

		await Verify(trimmedOutput)
			.UseDirectory("Snapshots/StructureVerification");
	}
	[Fact]
	public async Task VerifyDuplicateReferences_ShowsAllOccurrences()
	{
		var testFile = Path.Combine("TestScenarios", "02_DuplicateReferences", "root.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);
		var output = console.Output;

		// Trim trailing whitespace from each line to avoid test failures
		var trimmedOutput = TrimTrailingWhitespace(output);

		await Verify(trimmedOutput)
			.UseDirectory("Snapshots/StructureVerification");
	}

	[Fact]
	public async Task VerifyCircularReferences_ShowsWarningIcons()
	{
		var testFile = Path.Combine("TestScenarios", "03_CircularReferences", "root.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);

		var output = console.Output;

		await Verify(output)
			.UseDirectory("Snapshots/StructureVerification");
	}

	[Fact]
	public async Task VerifyFolderJumping_RelistsFolders()
	{
		var testFile = Path.Combine("TestScenarios", "04_FolderNavigation", "root.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);

		var output = console.Output;

		await Verify(output)
			.UseDirectory("Snapshots/StructureVerification");
	}

	[Fact]
	public async Task VerifyConsecutiveSameFolder_ShowsAsChildren()
	{
		var testFile = Path.Combine("TestScenarios", "05_ConsecutiveIncludes", "consecutive-same-folder.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);

		var output = console.Output;

		await Verify(output)
			.UseDirectory("Snapshots/StructureVerification");
	}

	[Fact]
	public async Task VerifyComplexStructure_AllFeaturesTogether()
	{
		var testFile = Path.Combine("TestScenarios", "Shared", "MainFolder", "complex-root.txt");
		var fileRelationships = BuildFileRelationships(testFile);

		var console = new TestConsole();
		var view = new StructureAndReferenceView(new FileInfo(testFile), fileRelationships);
		console.Write(view);

		var output = console.Output;

		await Verify(output)
			.UseDirectory("Snapshots/StructureVerification");
	}
	/// <summary>
	/// Trims all types of whitespace characters from the end of each line
	/// </summary>
	private static string TrimTrailingWhitespace(string output)
	{
		// Include common whitespace characters that might be added by Spectre.Console
		char[] whitespaceChars = { ' ', '\t', '\u00A0', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', '\u200B', '\u202F', '\u205F', '\u3000', '\uFEFF' };
		
		using var reader = new StringReader(output);
		var lines = new List<string>();
		string? line;
		while ((line = reader.ReadLine()) != null)
		{
			lines.Add(line.TrimEnd(whitespaceChars));
		}
		return string.Join('\n', lines);
	}

	[GeneratedRegex(@"^\s*#include\s+(.+)\s*$", RegexOptions.Compiled)]
	private static partial Regex IncludePattern();
}
