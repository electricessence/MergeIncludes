using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions;

/// <summary>
/// A renderable that creates clickable hyper-links for files and folders using Spectre.Console's native Style.link parameter
/// Only creates actual links when running in Windows Terminal or other terminals that support clickable links.
/// </summary>
public static class HyperLink
{
	/// <summary>
	/// Creates a hyperlinked markup with custom markup content
	/// </summary>
	/// <param name="path">The file or folder path to link to</param>
	/// <param name="markup">The markup content to display</param>
	/// <param name="style">Optional additional style to apply</param>
	/// <returns>A renderable with hyperlink (only if in Windows Terminal)</returns>
	public static IRenderable Markup(string path, string markup, Style? style = null)
	{
		ArgumentNullException.ThrowIfNull(markup);
		ArgumentException.ThrowIfNullOrEmpty(path);

		// Only create links when terminal supports them
		if (!TerminalCapabilities.Links)
		{
			return new Markup(markup, style ?? Style.Plain);
		}

		// Create style with link, combining with existing style if provided
		var linkStyle = style != null
			? new Style(style.Foreground, style.Background, style.Decoration, link: path)
			: new Style(link: path);
		return new Markup(markup, linkStyle);
	}

	/// <summary>
	/// Creates a hyperlinked text renderable
	/// </summary>
	/// <param name="path">The file or folder path to link to</param>
	/// <param name="text">The text to display</param>
	/// <param name="style">Optional additional style to apply</param>
	/// <returns>A renderable with hyperlink (only if in Windows Terminal)</returns>
	public static IRenderable For(string path, string text, Style? style = null)
	{
		ArgumentNullException.ThrowIfNull(text);
		ArgumentException.ThrowIfNullOrEmpty(path);

		// Only create links when terminal supports them
		if (!TerminalCapabilities.Links)
		{
			return new Text(text, style ?? Style.Plain);
		}

		// Create style with link, combining with existing style if provided
		var linkStyle = style != null
			? new Style(style.Foreground, style.Background, style.Decoration, link: path)
			: new Style(link: path);
		return new Text(text, linkStyle);
	}

	/// <summary>
	/// Creates a hyperlinked text renderable using the path as both link and text
	/// </summary>
	/// <param name="path">The file or folder path to link to and display</param>
	/// <param name="style">Optional additional style to apply</param>
	/// <returns>A renderable with hyperlink</returns>
	public static IRenderable For(string path, Style? style = null)
		=> For(path, path, style);

	/// <summary>
	/// Creates a hyperlinked text renderable for a file
	/// </summary>
	/// <param name="file">The file to link to</param>
	/// <param name="style">Optional additional style to apply</param>
	/// <returns>A renderable with hyperlink to the file</returns>
	public static IRenderable For(FileInfo file, Style? style = null)
	{
		ArgumentNullException.ThrowIfNull(file);
		return For(file.FullName, file.Name, style);
	}

	/// <summary>
	/// Creates a hyperlinked text renderable for a directory
	/// </summary>
	/// <param name="directory">The directory to link to</param>
	/// <param name="style">Optional additional style to apply</param>
	/// <returns>A renderable with hyperlink to the directory</returns>
	public static IRenderable For(DirectoryInfo directory, Style? style = null)
	{
		ArgumentNullException.ThrowIfNull(directory);
		return For(directory.FullName, directory.Name, style);
	}
}
