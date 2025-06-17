using Spectre.Console;
using System.Text;

namespace MergeIncludes.Renderables;

/// <summary>
/// Smart folder context and display logic for StructureAndReferenceView
/// </summary>
public sealed partial class StructureAndReferenceView
{
	/// <summary>
	/// Context tracker for smart folder display to avoid duplication and show proper hierarchy
	/// </summary>
	private class SmartFolderContext
	{
		public DirectoryInfo RootDirectory { get; }
		public string CurrentFolderPath { get; set; } = "";
		public int CurrentDepth { get; set; } = 0;

		private readonly HashSet<string> _displayedFolders;
		private readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _displayedFoldersLookup;

		public SmartFolderContext(DirectoryInfo rootDirectory)
		{
			RootDirectory = rootDirectory;
			var df = new HashSet<string>();
			_displayedFolders = df;
			_displayedFoldersLookup = df.GetAlternateLookup<ReadOnlySpan<char>>();
		}

		public bool WasDisplayed(ReadOnlySpan<char> path)
			=> _displayedFoldersLookup.Contains(path);

		public bool Add(string path)
			=> _displayedFolders.Add(path);

		public bool Add(ReadOnlySpan<char> path)
			=> !WasDisplayed(path) && _displayedFolders.Add(path.ToString());
	}

	/// <summary>
	/// Creates smart folder line that only shows folder names when path changes
	/// Shows proper tree hierarchy and avoids duplication
	/// </summary>
	private static Text CreateSmartFolderLine(string relativePath, SmartFolderContext context)
	{
		if (relativePath == context.CurrentFolderPath)
		{
			// Same folder as previous line - show continuation indicator
			var continuation = GetFolderContinuation(context.CurrentDepth);
			return new Text(continuation, new Style(Color.DarkGreen));
		}
		else
		{
			// Different folder - update context and determine what to show
			context.CurrentFolderPath = relativePath;

			if (string.IsNullOrEmpty(relativePath))
			{
				// Root folder file
				context.CurrentDepth = 0;
				return new Text("│", new Style(Color.DarkGreen));
			}
			else
			{
				// Sub-folder - determine what needs to be shown
				var count = 0;
				foreach (var _ in relativePath.AsSpan().Split('/')) count++;
				context.CurrentDepth = count;

				// Determine if this is a new folder branch or continuation
				var folderDisplay = DetermineSmartFolderDisplay(relativePath.AsSpan(), context);

				return new Text(folderDisplay, new Style(Color.Green));
			}
		}
	}

	/// <summary>
	/// Determines smart folder display based on context and path
	/// </summary>
	/// <param name="path">Path as a ReadOnlySpan of chars</param>
	/// <param name="context">The smart folder context</param>
	/// <returns>Formatted display string for the folder line</returns>
	private static string DetermineSmartFolderDisplay(ReadOnlySpan<char> path, SmartFolderContext context)
	{
		// Use a single StringBuilder instance to avoid multiple string allocations
		var builder = new StringBuilder(64);

		// Count path parts without allocating an array
		int depth = 1;
		for (int i = 0; i < path.Length; i++)
		{
			if (path[i] == '/')
				depth++;
		}

		// Get the last folder name without allocating
		ReadOnlySpan<char> lastFolder = path;
		int lastSlashIndex = path.LastIndexOf('/');
		if (lastSlashIndex >= 0)
		{
			lastFolder = path.Slice(lastSlashIndex + 1);
		}

		// Check if we've shown this folder path before
		if (context.Add(path))
		{
			if (depth > 1)
			{
				// Append indent spaces
				int indentSpaces = (depth - 1) * 2;
				builder.Append(' ', indentSpaces);
			}

			// Append connector and folder name
			builder.Append("├── 📁 ");
			builder.Append(lastFolder);
			builder.Append('/');
		}
		else
		{
			// This exact folder path was already shown - use continuation
			if (depth > 1)
			{
				// Append indent spaces
				int indentSpaces = (depth - 1) * 2;
				builder.Append(' ', indentSpaces);
			}

			builder.Append('│');
		}

		return builder.ToString();
	}

	/// <summary>
	/// Gets continuation indicator for files in the same folder
	/// </summary>
	private static string GetFolderContinuation(int depth)
	{
		if (depth == 0)
		{
			return "│"; // Root folder continuation
		}
		else
		{
			var indent = new string(' ', (depth - 1) * 2);
			return $"{indent}│";
		}
	}
}