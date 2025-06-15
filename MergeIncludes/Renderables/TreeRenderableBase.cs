using Spectre.Console;

namespace MergeIncludes.Renderables;

/// <summary>
/// Base class for tree-based renderables that need file/folder hyperlink functionality
/// </summary>
public abstract class TreeRenderableBase : RenderableBase
{
	/// <summary>
	/// Helper method to create a hyperlink markup string
	/// </summary>
	protected static string CreateHyperlinkMarkup(string uri, string displayText, string? style = null)
	{
		var escaped = displayText.EscapeMarkup();
		if (!string.IsNullOrEmpty(style))
		{
			return $"[{style}][link={uri}]{escaped}[/][/]";
		}

		return $"[link={uri}]{escaped}[/]";
	}

	/// <summary>
	/// Helper method to create a file URI
	/// </summary>
	protected static string CreateFileUri(string filePath)
	{
		try
		{
			return new System.Uri(Path.GetFullPath(filePath)).AbsoluteUri;
		}
		catch
		{
			return filePath; // Fallback
		}
	}

	/// <summary>
	/// Helper method to create a folder URI
	/// </summary>
	protected static string CreateFolderUri(string folderPath)
	{
		try
		{
			return new System.Uri(Path.GetFullPath(folderPath)).AbsoluteUri;
		}
		catch
		{
			return folderPath; // Fallback
		}
	}
}
