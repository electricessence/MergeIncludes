using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.IO;

namespace Spectre.Console.Extensions
{
    /// <summary>
    /// Utility methods for creating linked text paths for common file operations.
    /// </summary>
    public static class PathLink
    {
        /// <summary>
        /// Creates a linked text path for a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="style">Optional style to apply.</param>
        /// <returns>A renderable path with a link to the file.</returns>
        public static IRenderable File(string filePath, Style? style = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var linkPath = new LinkableTextPath(filePath, true);
            if (style != null)
            {
                return linkPath.LeafStyle(style);
            }
            return linkPath;
        }

        /// <summary>
        /// Creates a linked text path for a file with custom styling.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="rootColor">Color for the root segment.</param>
        /// <param name="separatorColor">Color for path separators.</param>
        /// <param name="stemColor">Color for middle segments.</param>
        /// <param name="leafColor">Color for the leaf segment.</param>
        /// <returns>A renderable path with a link to the file.</returns>
        public static IRenderable File(
            string filePath, 
            Color rootColor, 
            Color separatorColor, 
            Color stemColor, 
            Color leafColor)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            return new LinkableTextPath(filePath, true)
                .RootStyle(rootColor)
                .SeparatorStyle(separatorColor)
                .StemStyle(stemColor)
                .LeafStyle(leafColor);
        }

        /// <summary>
        /// Creates a linked text path for a directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="style">Optional style to apply.</param>
        /// <returns>A renderable path with a link to the directory.</returns>
        public static IRenderable Directory(string directoryPath, Style? style = null)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            var linkPath = new LinkableTextPath(directoryPath, true);
            if (style != null)
            {
                return linkPath.LeafStyle(style);
            }
            return linkPath;
        }

        /// <summary>
        /// Creates a linked text path for a FileInfo.
        /// </summary>
        /// <param name="fileInfo">The FileInfo object.</param>
        /// <param name="style">Optional style to apply.</param>
        /// <returns>A renderable path with a link to the file.</returns>
        public static IRenderable ForFile(FileInfo fileInfo, Style? style = null)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            return File(fileInfo.FullName, style);
        }

        /// <summary>
        /// Creates a linked text path for a DirectoryInfo.
        /// </summary>
        /// <param name="directoryInfo">The DirectoryInfo object.</param>
        /// <param name="style">Optional style to apply.</param>
        /// <returns>A renderable path with a link to the directory.</returns>
        public static IRenderable ForDirectory(DirectoryInfo directoryInfo, Style? style = null)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException(nameof(directoryInfo));
            }

            return Directory(directoryInfo.FullName, style);
        }

        /// <summary>
        /// Creates a linked text path with a custom URL.
        /// </summary>
        /// <param name="path">The path text to display.</param>
        /// <param name="url">The URL to link to.</param>
        /// <param name="style">Optional style to apply.</param>
        /// <returns>A renderable path with a custom link.</returns>
        public static IRenderable Custom(string path, string url, Style? style = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var linkPath = new LinkableTextPath(path, url);
            if (style != null)
            {
                return linkPath.LeafStyle(style);
            }
            return linkPath;
        }

        /// <summary>
        /// Gets a predefined style for a given file extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>A style appropriate for the file type.</returns>
        public static Style GetFileTypeStyle(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return Style.Plain;
            }

            extension = extension.TrimStart('.').ToLowerInvariant();

            return extension switch
            {
                "cs" => new Style(foreground: Color.Green1),
                "fs" => new Style(foreground: Color.Purple),
                "vb" => new Style(foreground: Color.Blue),
                "csproj" or "fsproj" or "vbproj" or "sln" => new Style(foreground: Color.Fuchsia),
                "json" => new Style(foreground: Color.Yellow),
                "xml" or "xaml" => new Style(foreground: Color.Orange1),
                "txt" => new Style(foreground: Color.Grey),
                "md" or "markdown" => new Style(foreground: Color.Aqua),
                "html" or "htm" => new Style(foreground: Color.Orange3),
                "css" => new Style(foreground: Color.Blue),
                "js" or "ts" => new Style(foreground: Color.Yellow3),
                "py" => new Style(foreground: Color.Blue1),
                "dll" or "exe" => new Style(foreground: Color.Red),
                "png" or "jpg" or "jpeg" or "gif" or "bmp" => new Style(foreground: Color.Magenta1),
                _ => Style.Plain
            };
        }

        /// <summary>
        /// Creates a linked text path with file type coloring based on extension.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A renderable path with a link to the file and appropriate coloring.</returns>
        public static IRenderable Smart(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            string extension = Path.GetExtension(filePath);
            Style fileStyle = GetFileTypeStyle(extension);
            
            return new LinkableTextPath(filePath, true)
                .RootStyle(Color.Blue)
                .SeparatorStyle(Color.Grey)
                .StemStyle(Color.DarkGreen)
                .LeafStyle(fileStyle);
        }
    }
}