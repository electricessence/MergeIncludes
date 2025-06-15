using Spectre.Console;
using Spectre.Console.Rendering;

namespace MergeIncludes.Renderables;

/// <summary>
/// A renderable that creates clickable hyper-links for files and folders
/// </summary>
/// <remarks>
/// Creates a new hyper-link renderable
/// </remarks>
/// <param name="path">The file or folder path to link to</param>
/// <param name="label">Optional custom label (defaults to filename/foldername)</param>
/// <param name="style">Optional style for the link</param>
public class HyperLink(string path, string? label = null, Style? style = null) : RenderableBase
{
	private readonly IRenderable _text = new Text(label ?? path, style);

	private static readonly bool IsWindowsTerminal = Environment.GetEnvironmentVariable("WT_SESSION") != null;

	public override Measurement Measure(RenderOptions options, int maxWidth)
		=> _text.Measure(options, maxWidth);  // ← works fine via interface

	public override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
	{
		yield return new Segment($"\u001b]8;;{path}\u0007");
		foreach (var segment in _text.Render(options, maxWidth))
			yield return segment;
		yield return new Segment("\u001b]8;;\u0007");
	}

	public static IRenderable CreateMarkup(string path, string? label = null, Style? style = null)
	{
		ArgumentNullException.ThrowIfNull(path);
		ArgumentException.ThrowIfNullOrWhiteSpace(label);
		return IsWindowsTerminal
			? new Markup($"[link={Markup.Escape(path)}]{Markup.Escape(label ?? path)}[/]", style)
			: new Text(label ?? path, style);
	}
}
