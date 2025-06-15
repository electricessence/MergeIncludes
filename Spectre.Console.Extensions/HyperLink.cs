using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions;

/// <summary>
/// A renderable that creates clickable hyper-links for files and folders
/// </summary>
/// <remarks>
/// Creates a new hyper-link renderable
/// </remarks>
/// <param name="path">The file or folder path to link to</param>
/// <param name="label">Custom label</param>
public static class HyperLink
{
	private static readonly bool IsWindowsTerminal
		= Environment.GetEnvironmentVariable("WT_SESSION") != null;

	public static IRenderable Markup(string path, string markup, Style? style = null)
	{
		ArgumentNullException.ThrowIfNull(markup);
		ArgumentException.ThrowIfNullOrEmpty(path);
		if (!IsWindowsTerminal) return new Markup(markup, style);
		
		// Use the path directly with no modifications
		return new Markup($"[link={Console.Markup.Escape(path)}]{markup}[/]", style);
	}

	public static IRenderable For(string path, string text, Style? style = null)
	{
		ArgumentNullException.ThrowIfNull(text);
		ArgumentException.ThrowIfNullOrEmpty(path);
		if (!IsWindowsTerminal) return new Text(text, style);
		
		// Use the path directly with no modifications
		return new Markup($"[link={Console.Markup.Escape(path)}]{Console.Markup.Escape(text)}[/]", style);
	}

	public static IRenderable For(string path, Style? style = null)
		=> For(path, path, style);

	public static IRenderable For(FileInfo file, Style? style = null)
	{
		ArgumentNullException.ThrowIfNull(file);
		// Use the full path directly with no modifications
		return For(file.FullName, file.Name, style);
	}

	public static IRenderable For(DirectoryInfo directory, Style? style = null)
	{
		ArgumentNullException.ThrowIfNull(directory);
		// Use the full path directly with no modifications
		return For(directory.FullName, directory.Name, style);
	}
}
