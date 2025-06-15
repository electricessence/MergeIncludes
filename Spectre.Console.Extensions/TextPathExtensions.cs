using Spectre.Console;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions;

/// <summary>
/// Extension methods for TextPath.
/// </summary>
public static class TextPathExtensions
{    /// <summary>
    /// Creates a LinkableTextPath from a TextPath with the specified link URL.
    /// Note: This method cannot extract the path from an existing TextPath,
    /// so it's recommended to use CreateLinkableTextPath methods instead.
    /// </summary>
    /// <param name="textPath">The text path (used for reference only).</param>
    /// <param name="linkUrl">The URL to link to.</param>
    /// <returns>A LinkableTextPath instance or the original TextPath if path cannot be extracted.</returns>
    public static IRenderable ToLink(this TextPath textPath, string linkUrl)
    {
        if (textPath == null)
        {
            throw new ArgumentNullException(nameof(textPath));
        }

        if (string.IsNullOrEmpty(linkUrl))
        {
            return textPath;
        }        // Since we can't easily extract the path from TextPath without reflection,
        // and reflection is fragile, we return the original TextPath.
        // Use CreateLinkableTextPath methods for new linked paths instead.
        return textPath;
    }
    
    /// <summary>
    /// Creates a linkable version of a TextPath. Since we can't extract the path easily,
    /// this returns the original TextPath. Use CreateLinkableTextPath methods instead for linked paths.
    /// </summary>
    /// <param name="textPath">The text path.</param>
    /// <returns>The original TextPath (no link functionality without path access).</returns>
    public static IRenderable AsLink(this TextPath textPath)
    {
        if (textPath == null)
        {
            throw new ArgumentNullException(nameof(textPath));
        }
        
        // Without access to the internal path, we can't create a link
        // Use CreateLinkableTextPath methods instead for new linked paths
        return textPath;
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
}