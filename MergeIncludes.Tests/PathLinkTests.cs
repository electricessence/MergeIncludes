using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Testing;

namespace MergeIncludes.Tests;

/// <summary>
/// Tests for the PathLink utility class
/// </summary>
[UsesVerify]
public class PathLinkTests
{
	[Fact]
	public async Task PathLink_File_CreatesProperlyFormattedLink()
	{
		// Arrange
		var filePath = @"C:\Some\Test\Path\document.txt";

		// Act
		var renderable = PathLink.File(filePath);
		var console = new TestConsole();
		console.Write(renderable);

		// Assert using Verify
		await Verify(console.Output)
			.UseDirectory("Snapshots")
			.UseFileName("PathLink_File_Basic");
	}

	[Fact]
	public async Task PathLink_File_WithCustomColors_AppliesColorsCorrectly()
	{
		// Arrange
		var filePath = @"C:\Some\Test\Path\document.txt";

		// Act
		var renderable = PathLink.File(
			filePath,
			Color.Red,    // Root color
			Color.White,  // Separator color
			Color.Blue,   // Stem color
			Color.Green); // Leaf color

		var console = new TestConsole();
		console.Write(renderable);

		// Assert using Verify
		await Verify(console.Output)
			.UseDirectory("Snapshots")
			.UseFileName("PathLink_File_CustomColors");
	}

	[Fact]
	public async Task PathLink_Smart_AppliesCorrectColoringBasedOnExtension()
	{
		// Arrange - Create multiple paths with different extensions
		var paths = new[]
		{
			@"C:\Project\Program.cs",
			@"C:\Project\Page.html",
			@"C:\Project\Data.json",
			@"C:\Project\Readme.md",
			@"C:\Project\Image.png",
			@"C:\Project\Unknown.xyz"
		};

		// Act - Render each with Smart method
		var results = new Dictionary<string, string>();

		foreach (var path in paths)
		{
			var renderable = PathLink.Smart(path);
			var console = new TestConsole();
			console.Write(renderable);
			results[Path.GetExtension(path)] = console.Output;
		}

		// Assert using Verify
		await Verify(results)
			.UseDirectory("Snapshots")
			.UseFileName("PathLink_Smart_ExtensionColoring");
	}

	[Fact]
	public async Task PathLink_Custom_CreatesLinkWithCustomUrl()
	{
		// Arrange
		var path = @"C:\Some\Path\document.txt";
		var url = "https://example.com/view?file=document.txt";

		// Act
		var renderable = PathLink.Custom(path, url);
		var console = new TestConsole();
		console.Write(renderable);

		// Assert using Verify
		await Verify(console.Output)
			.UseDirectory("Snapshots")
			.UseFileName("PathLink_Custom_Url");
	}

	[Fact]
	public async Task PathLink_ForFile_CreatesLinkFromFileInfo()
	{
		// Arrange
		var fileInfo = new FileInfo(@"C:\Some\Path\document.txt");

		// Act
		var renderable = PathLink.ForFile(fileInfo);
		var console = new TestConsole();
		console.Write(renderable);

		// Assert using Verify
		await Verify(console.Output)
			.UseDirectory("Snapshots")
			.UseFileName("PathLink_ForFile");
	}

	[Fact]
	public async Task PathLink_ForDirectory_CreatesLinkFromDirectoryInfo()
	{
		// Arrange
		var dirInfo = new DirectoryInfo(@"C:\Some\Project\Folder");

		// Act
		var renderable = PathLink.ForDirectory(dirInfo);
		var console = new TestConsole();
		console.Write(renderable);

		// Assert using Verify
		await Verify(console.Output)
			.UseDirectory("Snapshots")
			.UseFileName("PathLink_ForDirectory");
	}
}