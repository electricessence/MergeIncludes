# Spectre.Console.Extensions

This library extends [Spectre.Console](https://github.com/spectreconsole/spectre.console) with additional functionality and utility classes.

## Features

### LinkableTextPath

`LinkableTextPath` enhances Spectre.Console's `TextPath` with clickable hyperlink functionality. This is especially useful in terminals that support hyperlinks, like Windows Terminal.

#### Usage
// Create a clickable text path
var path = new LinkableTextPath("/path/to/file.txt", true) // true = link to self
    .RootStyle(Color.Blue)
    .SeparatorStyle(Color.Grey)
    .StemStyle(Color.Green)
    .LeafStyle(Color.Yellow);

// Write to console
console.Write(path);
### TextPathExtensions

Extension methods for working with `TextPath` objects:
// Convert a TextPath to a linked version by specifying the URL
var textPath = new TextPath("/path/to/file.txt")
    .RootStyle(Color.Blue);

var linkedPath1 = textPath.ToLink("https://example.com");
console.Write(linkedPath1);

// Or more conveniently, create a link to the path itself with AsLink()
var linkedPath2 = textPath.AsLink();
console.Write(linkedPath2);

// Create from FileInfo
var fileInfo = new FileInfo("/path/to/file.txt");
console.Write(fileInfo.CreateLinkableTextPath());
### PathLink

The `PathLink` utility class provides convenient methods for creating linked text paths with appropriate styling:
// Basic file link
console.Write(PathLink.File("/path/to/file.txt"));

// Directory link
console.Write(PathLink.Directory("/path/to/folder"));

// Smart styling based on file extension
console.Write(PathLink.Smart("/path/to/file.cs")); 

// Custom link URL
console.Write(PathLink.Custom("/path/to/file.txt", "https://example.com"));

// With custom colors
console.Write(PathLink.File(
    "/path/to/file.txt", 
    Color.Red,    // Root color
    Color.White,  // Separator color
    Color.Blue,   // Stem color
    Color.Green   // Leaf color
));
### PanelBuilder

The `PanelBuilder` class allows for easy building of panels with multiple elements:
var builder = new PanelBuilder("Header Text");

// Add items
builder.Add(new Markup("[blue]Item 1[/]"));
builder.Add(new Text("Item 2"));

// Add multiple items
builder.Add(new[] {
    new Markup("[green]Item 3[/]"),
    new Rule(),
    new Text("Item 4")
});

// Write to console
console.Write(builder);
## Terminal Compatibility

The link functionality works best in terminals that support hyperlinks, like Windows Terminal. In other terminals, the paths will be displayed without links but will still maintain their styling.

## Path Handling

When using paths for links, the paths are used exactly as provided without any modification or URL formatting. This ensures the original path is preserved in the link, maintaining compatibility with different terminals and platforms. For example:
- A path like `C:\Users\Documents\file.txt` will be linked exactly as `C:\Users\Documents\file.txt`
- No `file:///` prefix or other formatting is added to the path

This approach provides the most consistent behavior across different terminal implementations.