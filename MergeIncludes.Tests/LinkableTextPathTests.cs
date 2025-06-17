using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Testing;

namespace MergeIncludes.Tests;

/// <summary>
/// Tests for the LinkableTextPath class using Verify for snapshot testing
/// </summary>
[UsesVerify]
public class LinkableTextPathTests
{
	[Fact]
	public async Task LinkableTextPath_RendersProperly()
	{
		// Arrange
		var testPath = @"C:\Some\Test\Path\file.txt";
		var linkableTextPath = new LinkableTextPath(testPath, true)
			.RootStyle(Color.Blue)
			.SeparatorStyle(Color.Grey)
			.StemStyle(Color.Green)
			.LeafStyle(Color.Yellow);

		// Act
		var console = new TestConsole();
		console.Write(linkableTextPath);
		var output = console.Output;

		// Assert using Verify
		await Verify(output)
			.UseDirectory("Snapshots")
			.UseFileName("LinkableTextPath_Rendered");
	}

	[Fact]
	public async Task TextPathExtensions_ToLink_CreatesProperly()
	{
		// Arrange
		var testPath = @"C:\Some\Test\Path\file.txt";
		var textPath = new TextPath(testPath)
			.RootStyle(Color.Blue)
			.SeparatorStyle(Color.Grey)
			.StemStyle(Color.Green)
			.LeafStyle(Color.Yellow);

		// Act
		var linkRenderable = textPath.ToLink(testPath);
		var console = new TestConsole();
		console.Write(linkRenderable);
		var output = console.Output;

		// Assert using Verify
		await Verify(output)
			.UseDirectory("Snapshots")
			.UseFileName("TextPath_ToLink_Rendered");
	}

	[Fact]
	public async Task TextPathExtensions_AsLink_CreatesProperly()
	{
		// Arrange
		var testPath = @"C:\Some\Test\Path\file.txt";
		var textPath = new TextPath(testPath)
			.RootStyle(Color.Blue)
			.SeparatorStyle(Color.Grey)
			.StemStyle(Color.Green)
			.LeafStyle(Color.Yellow);

		// Act
		var linkRenderable = textPath.AsLink();
		var console = new TestConsole();
		console.Write(linkRenderable);
		var output = console.Output;

		// Assert using Verify
		await Verify(output)
			.UseDirectory("Snapshots")
			.UseFileName("TextPath_AsLink_Rendered");
	}

	[Fact]
	public async Task CreateLinkableTextPath_FromFileInfo_CreatesProperly()
	{
		// Arrange - Use a fixed path instead of temp file for consistent snapshots
		var filePath = @"C:\Test\Example.txt";
		var fileInfo = new FileInfo(filePath);

		// Act
		var linkableTextPath = fileInfo.CreateLinkableTextPath();
		var console = new TestConsole();
		console.Write(linkableTextPath);
		var output = console.Output;

		// Assert using Verify
		await Verify(output)
			.UseDirectory("Snapshots")
			.UseFileName("FileInfo_CreateLinkableTextPath_Rendered");
	}

	[Fact]
	public async Task LinkableTextPath_WithAndWithoutLink_DifferentOutput()
	{
		// Arrange - Create two paths, one with link and one without
		var testPath = @"C:\Some\Test\Path\file.txt";

		var linkedPath = new LinkableTextPath(testPath, true)
			.RootStyle(Color.Blue)
			.SeparatorStyle(Color.Grey)
			.StemStyle(Color.Green)
			.LeafStyle(Color.Yellow);

		var nonLinkedPath = new LinkableTextPath(testPath, false)
			.RootStyle(Color.Blue)
			.SeparatorStyle(Color.Grey)
			.StemStyle(Color.Green)
			.LeafStyle(Color.Yellow);

		// Act
		var consoleLinked = new TestConsole();
		consoleLinked.Write(linkedPath);
		var outputLinked = consoleLinked.Output;

		var consoleNonLinked = new TestConsole();
		consoleNonLinked.Write(nonLinkedPath);
		var outputNonLinked = consoleNonLinked.Output;

		// Create a combined object with both outputs for verification
		var result = new
		{
			WithLink = outputLinked,
			WithoutLink = outputNonLinked
		};

		// Assert using Verify
		await Verify(result)
			.UseDirectory("Snapshots")
			.UseFileName("LinkableTextPath_WithAndWithoutLink");
	}

	[Fact]
	public async Task LinkableTextPath_WithCustomLinkUrl_ContainsCorrectMarkup()
	{
		// Arrange
		var testPath = @"C:\Some\Test\Path\file.txt";
		var customUrl = "https://example.com/files";

		var linkableTextPath = new LinkableTextPath(testPath, customUrl)
			.RootStyle(Color.Blue)
			.SeparatorStyle(Color.Grey)
			.StemStyle(Color.Green)
			.LeafStyle(Color.Yellow);

		// Act
		var console = new TestConsole();
		console.Write(linkableTextPath);
		var output = console.Output;

		// Assert using Verify
		await Verify(output)
			.UseDirectory("Snapshots")
			.UseFileName("LinkableTextPath_CustomUrl");
	}

	[Fact]
	public async Task HyperLink_WindowsTerminalDetection_WorksCorrectly()
	{
		// Get access to the private IsWindowsTerminal field via reflection
		var originalTerminalEnvironment = Environment.GetEnvironmentVariable("WT_SESSION");
		try
		{
			// Check behavior with and without Windows Terminal environment

			// 1. Test without Windows Terminal
			Environment.SetEnvironmentVariable("WT_SESSION", null);

			// Directly get the specified file path
			var file = new FileInfo(@"C:\Test\Document.txt");
			var nonWtLink = HyperLink.For(file);

			// 2. Test with Windows Terminal
			Environment.SetEnvironmentVariable("WT_SESSION", "test-session");

			var wtLink = HyperLink.For(file);

			// Render both to compare
			var console1 = new TestConsole();
			var console2 = new TestConsole();

			console1.Write(nonWtLink);
			console2.Write(wtLink);

			var result = new
			{
				WithoutWindowsTerminal = console1.Output,
				WithWindowsTerminal = console2.Output
			};

			// Verify both outputs
			await Verify(result)
				.UseDirectory("Snapshots")
				.UseFileName("HyperLink_WindowsTerminalDetection");
		}
		finally
		{
			// Restore original environment
			Environment.SetEnvironmentVariable("WT_SESSION", originalTerminalEnvironment);
		}
	}

	[Fact]
	public async Task TextPathExtensions_AsLink_UsesPathReflection_WorksCorrectly()
	{
		// Arrange - Create a TextPath and then use AsLink which uses reflection
		var testPath = @"C:\Some\Test\Path\file.txt";
		var textPath = new TextPath(testPath)
			.RootStyle(Color.Blue)
			.SeparatorStyle(Color.Grey)
			.StemStyle(Color.Green)
			.LeafStyle(Color.Yellow);

		// Act - Convert to link using AsLink (which depends on reflection)
		var asLink = textPath.AsLink();

		// Also create the same path using ToLink for comparison
		var toLink = textPath.ToLink(testPath);

		var console1 = new TestConsole();
		var console2 = new TestConsole();

		console1.Write(asLink);
		console2.Write(toLink);

		var result = new
		{
			UsingAsLink = console1.Output,
			UsingToLink = console2.Output
		};

		// Assert - Both should produce the same output
		await Verify(result)
			.UseDirectory("Snapshots")
			.UseFileName("AsLink_VS_ToLink_Comparison");
	}

	[Fact]
	public async Task LinkableTextPath_TruncatedPath_PreservesCorrectLinks()
	{
		// Arrange - Use a very long path that will likely get truncated
		var longPath = @"C:\Very\Long\Path\With\Many\Segments\That\Should\Get\Truncated\When\Rendered\In\A\Small\Width\final-file.txt";
		var linkableTextPath = new LinkableTextPath(longPath, true)
			.RootStyle(Color.Blue)
			.SeparatorStyle(Color.Grey)
			.StemStyle(Color.Green)
			.LeafStyle(Color.Yellow);

		// Act - Render with a very small width to force truncation
		var console = new TestConsole().Width(40); // Small width to force truncation
		console.Write(linkableTextPath);
		var output = console.Output;

		// Assert using Verify
		await Verify(output)
			.UseDirectory("Snapshots")
			.UseFileName("LinkableTextPath_TruncatedPath_CorrectLinks");
	}

	[Fact]
	public async Task LinkableTextPath_TruncatedPath_LinksAnalysis()
	{
		// Arrange - Test specifically the link preservation with truncation
		var testPath = @"C:\Development\Projects\MyApp\src\Components\UserInterface\Views\Settings\UserPreferences.cs";
		var linkableTextPath = new LinkableTextPath(testPath, true);

		// Act - Render with different widths to see truncation behavior
		var results = new Dictionary<string, string>();
		
		foreach (var width in new[] { 20, 40, 60, 80, 120 })
		{
			var console = new TestConsole().Width(width);
			console.Write(linkableTextPath);
			results[$"Width_{width}"] = console.Output;
		}

		// Assert using Verify
		await Verify(results)
			.UseDirectory("Snapshots")
			.UseFileName("LinkableTextPath_TruncatedPath_Analysis");
	}
}