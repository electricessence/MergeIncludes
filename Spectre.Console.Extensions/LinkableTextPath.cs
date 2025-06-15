using Spectre.Console;
using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions;

/// <summary>
/// A text path with link capability.
/// </summary>
/// <remarks>
/// LinkableTextPath enhances Spectre.Console's TextPath with clickable links capability.
/// This is especially useful in terminals that support hyperlinks, like Windows Terminal.
/// 
/// There are several ways to create and work with linked text paths:
/// 
/// 1. Direct instantiation:
///    <code>
///    var path = new LinkableTextPath("/path/to/file.txt", true) // true = link to self
///        .RootStyle(Color.Blue)
///        .SeparatorStyle(Color.Grey)
///        .StemStyle(Color.Green)
///        .LeafStyle(Color.Yellow);
///    console.Write(path);
///    </code>
///    
/// 2. Converting existing TextPath:
///    <code>
///    var textPath = new TextPath("/path/to/file.txt")
///        .RootStyle(Color.Blue);
///    console.Write(textPath.ToLink(url));
///    </code>
///    
/// 3. Using extension methods:
///    <code>
///    var fileInfo = new FileInfo("/path/to/file.txt");
///    console.Write(fileInfo.CreateLinkableTextPath());
///    </code>
///    
/// 4. Using the PathLink utility class:
///    <code>
///    console.Write(PathLink.File("/path/to/file.txt"));
///    console.Write(PathLink.Smart("/path/to/file.cs")); // Uses extension-specific styling
///    </code>
/// </remarks>
public sealed class LinkableTextPath : IRenderable
{
	private readonly string _path;
	private readonly string? _linkUrl;
	private Style? _rootStyle;
	private Style? _separatorStyle;
	private Style? _stemStyle;
	private Style? _leafStyle;

	/// <summary>
	/// Creates a new instance of <see cref="LinkableTextPath"/>.
	/// </summary>
	/// <param name="path">The path.</param>
	public LinkableTextPath(string path)
		: this(path, null)
	{
	}

	/// <summary>
	/// Creates a new instance of <see cref="LinkableTextPath"/> with a link URL.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <param name="linkUrl">The URL to link to.</param>
	public LinkableTextPath(string path, string? linkUrl)
	{
		_path = path ?? throw new ArgumentNullException(nameof(path));
		_linkUrl = linkUrl;
	}

	/// <summary>
	/// Creates a new instance of <see cref="LinkableTextPath"/> with a link URL.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <param name="linkToSelf">If true, links to the path itself.</param>
	public LinkableTextPath(string path, bool linkToSelf)
		: this(path, linkToSelf ? path : null)
	{
	}

	/// <summary>
	/// Sets the style of the path's root.
	/// </summary>
	/// <param name="style">The root style.</param>
	/// <returns>The same instance so that multiple calls can be chained.</returns>
	public LinkableTextPath RootStyle(Style style)
	{
		_rootStyle = style;
		return this;
	}

	/// <summary>
	/// Sets the style of the path's root.
	/// </summary>
	/// <param name="color">The root color.</param>
	/// <returns>The same instance so that multiple calls can be chained.</returns>
	public LinkableTextPath RootStyle(Color color)
	{
		_rootStyle = new Style(foreground: color);
		return this;
	}

	/// <summary>
	/// Sets the style of path separators.
	/// </summary>
	/// <param name="style">The separator style.</param>
	/// <returns>The same instance so that multiple calls can be chained.</returns>
	public LinkableTextPath SeparatorStyle(Style style)
	{
		_separatorStyle = style;
		return this;
	}

	/// <summary>
	/// Sets the style of path separators.
	/// </summary>
	/// <param name="color">The separator color.</param>
	/// <returns>The same instance so that multiple calls can be chained.</returns>
	public LinkableTextPath SeparatorStyle(Color color)
	{
		_separatorStyle = new Style(foreground: color);
		return this;
	}

	/// <summary>
	/// Sets the style of inner path segments.
	/// </summary>
	/// <param name="style">The stem style.</param>
	/// <returns>The same instance so that multiple calls can be chained.</returns>
	public LinkableTextPath StemStyle(Style style)
	{
		_stemStyle = style;
		return this;
	}

	/// <summary>
	/// Sets the style of inner path segments.
	/// </summary>
	/// <param name="color">The stem color.</param>
	/// <returns>The same instance so that multiple calls can be chained.</returns>
	public LinkableTextPath StemStyle(Color color)
	{
		_stemStyle = new Style(foreground: color);
		return this;
	}

	/// <summary>
	/// Sets the style of the last path segment.
	/// </summary>
	/// <param name="style">The leaf style.</param>
	/// <returns>The same instance so that multiple calls can be chained.</returns>
	public LinkableTextPath LeafStyle(Style style)
	{
		_leafStyle = style;
		return this;
	}

	/// <summary>
	/// Sets the style of the last path segment.
	/// </summary>
	/// <param name="color">The leaf color.</param>
	/// <returns>The same instance so that multiple calls can be chained.</returns>
	public LinkableTextPath LeafStyle(Color color)
	{
		_leafStyle = new Style(foreground: color);
		return this;
	}

	/// <inheritdoc/>
	public Measurement Measure(RenderOptions options, int maxWidth)
	{
		// Create a TextPath and apply our styles, then use its measurement
		var textPath = new TextPath(_path);

		if (_rootStyle != null) textPath.RootStyle(_rootStyle);
		if (_separatorStyle != null) textPath.SeparatorStyle(_separatorStyle);
		if (_stemStyle != null) textPath.StemStyle(_stemStyle);
		if (_leafStyle != null) textPath.LeafStyle(_leafStyle);

		return textPath.Measure(options, maxWidth);
	}

	/// <inheritdoc/>
	public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
	{
		// Create a TextPath and apply our styles
		var textPath = new TextPath(_path);

		if (_rootStyle != null) textPath.RootStyle(_rootStyle);
		if (_separatorStyle != null) textPath.SeparatorStyle(_separatorStyle);
		if (_stemStyle != null) textPath.StemStyle(_stemStyle);
		if (_leafStyle != null) textPath.LeafStyle(_leafStyle);

		// If no link URL is specified, just render the text path
		if (string.IsNullOrEmpty(_linkUrl))
		{
			return textPath.Render(options, maxWidth);
		}

		// Get the segments from the text path
		var segments = textPath.Render(options, maxWidth).ToList();

		// For each segment, add a link decoration
		return segments.Select(segment =>
		{
			if (segment.IsControlCode)
			{
				return segment;
			}

			// If the segment is already styled, apply the link as a decoration while preserving existing style
			if (segment.Style != Style.Plain)
			{
				// Create a new style by applying the link decoration to the existing style
				var style = new Style(
					segment.Style.Foreground,
					segment.Style.Background,
					segment.Style.Decoration,
					link: _linkUrl);

				return new Segment(segment.Text, style);
			}

			// If the segment has no style, create a new one with just the link
			return new Segment(
				segment.Text,
				new Style(link: _linkUrl));
		});
	}
}