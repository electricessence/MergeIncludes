using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions;

/// <summary>
/// Extension methods for TextPath.
/// </summary>
public static class TextPathExtensions
{
    /// <summary>
    /// Creates a LinkableTextPath from a TextPath with the specified link URL.
    /// </summary>
    /// <param name="textPath">The text path to create a link from.</param>
    /// <param name="linkUrl">The URL to link to.</param>
    /// <returns>A LinkableTextPath instance.</returns>
    public static IRenderable ToLink(this TextPath textPath, string linkUrl)
    {
        // TextPath doesn't expose its path, so we need to create a new LinkableTextPath
        // The user will need to re-apply styling
        if (textPath == null)
        {
            throw new ArgumentNullException(nameof(textPath));
        }

        // Create a new markup with link
        if (string.IsNullOrEmpty(linkUrl))
        {
            return textPath;
        }

        // Get a console to use for rendering
        var console = AnsiConsole.Console;
        var width = console.Profile.Width;
        
        // Use the console's capabilities to create render options
        var options = new RenderOptions(
            console.Profile.Capabilities, 
            new Size(width, 0) // Create a Size object instead of using int directly
        );
        
        var segments = textPath.Render(options, width);
        
        var markupBuilder = new System.Text.StringBuilder();
        
        markupBuilder.Append($"[link={Markup.Escape(linkUrl)}]");
        
        foreach (var segment in segments)
        {
            if (segment.IsControlCode)
            {
                markupBuilder.Append(segment.Text);
                continue;
            }

            var text = Markup.Escape(segment.Text);
            if (segment.Style != Style.Plain)
            {
                var style = segment.Style;
                string styleTag = StyleToMarkup(style);
                if (!string.IsNullOrEmpty(styleTag))
                {
                    markupBuilder.Append($"[{styleTag}]{text}[/]");
                }
                else
                {
                    markupBuilder.Append(text);
                }
            }
            else
            {
                markupBuilder.Append(text);
            }
        }
        
        markupBuilder.Append("[/]"); // Close link tag
        
        return new Markup(markupBuilder.ToString());
    }
    
    /// <summary>
    /// Converts a TextPath to a LinkableTextPath with a link to the path itself.
    /// This is a convenience method for common usage pattern.
    /// </summary>
    /// <param name="textPath">The text path to create a link from.</param>
    /// <returns>A renderable with the TextPath styling and a link to the path.</returns>
    /// <remarks>
    /// This is useful when you want to quickly convert an existing TextPath to a clickable one:
    /// <code>
    /// var path = new TextPath(@"C:\path\to\file.txt")
    ///     .RootStyle(Color.Blue)
    ///     .SeparatorStyle(Color.Grey);
    /// 
    /// // Instead of path.ToLink(@"C:\path\to\file.txt"):
    /// console.Write(path.AsLink());
    /// </code>
    /// </remarks>
    public static IRenderable AsLink(this TextPath textPath)
    {
        if (textPath == null)
        {
            throw new ArgumentNullException(nameof(textPath));
        }
        
        // Extract the original path using reflection (TextPath doesn't expose this publicly)
        // This is a safer approach than reconstructing the path
        var pathField = typeof(TextPath).GetField("_path", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        string? path = pathField?.GetValue(textPath) as string;
        
        if (string.IsNullOrEmpty(path))
        {
            // Fallback - if we can't get the path, create a link without an actual URL
            // The styling will be preserved but it won't be clickable
            return textPath;
        }
        
        return ToLink(textPath, path);
    }

    /// <summary>
    /// Creates a LinkableTextPath from a file path with a link to itself.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A LinkableTextPath instance.</returns>
    public static IRenderable CreateLinkableTextPath(string filePath)
    {
        return new LinkableTextPath(filePath, true);
    }

    /// <summary>
    /// Creates a LinkableTextPath from a file path with a link to the specified URL.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="linkUrl">The URL to link to.</param>
    /// <returns>A LinkableTextPath instance.</returns>
    public static IRenderable CreateLinkableTextPath(string filePath, string linkUrl)
    {
        return new LinkableTextPath(filePath, linkUrl);
    }

    /// <summary>
    /// Creates a LinkableTextPath from a file info with a link to itself.
    /// </summary>
    /// <param name="fileInfo">The file info.</param>
    /// <returns>A LinkableTextPath instance.</returns>
    public static IRenderable CreateLinkableTextPath(this FileInfo fileInfo)
    {
        return new LinkableTextPath(fileInfo.FullName, fileInfo.FullName);
    }

    /// <summary>
    /// Creates a LinkableTextPath from a directory info with a link to itself.
    /// </summary>
    /// <param name="directoryInfo">The directory info.</param>
    /// <returns>A LinkableTextPath instance.</returns>
    public static IRenderable CreateLinkableTextPath(this DirectoryInfo directoryInfo)
    {
        return new LinkableTextPath(directoryInfo.FullName, directoryInfo.FullName);
    }

    /// <summary>
    /// Converts a Style to its markup representation.
    /// </summary>
    private static string StyleToMarkup(Style style)
    {
        var parts = new List<string>();

        // Use pattern matching to safely handle nullable Color values
        if (style.Foreground is Color foreground)
        {
            parts.Add(foreground.ToString().ToLowerInvariant());
        }
        
        if (style.Background is Color background)
        {
            parts.Add("on " + background.ToString().ToLowerInvariant());
        }

        if (style.Decoration.HasFlag(Decoration.Bold))
        {
            parts.Add("bold");
        }
        
        if (style.Decoration.HasFlag(Decoration.Italic))
        {
            parts.Add("italic");
        }
        
        if (style.Decoration.HasFlag(Decoration.Underline))
        {
            parts.Add("underline");
        }
        
        if (style.Decoration.HasFlag(Decoration.Strikethrough))
        {
            parts.Add("strikethrough");
        }
        
        return string.Join(" ", parts);
    }
}