# Spectre.Console Renderables Guide

## Understanding the Rendering Pipeline

Based on studying the Spectre.Console source code, here's how the rendering
system works:

### Core Interfaces

1. **IRenderable** - Base interface for all renderable objects
   ```csharp
   public interface IRenderable
   {
       Measurement Measure(RenderOptions options, int maxWidth);
       IEnumerable<Segment> Render(RenderOptions options, int maxWidth);
   }
   ```

2. **Measurement** - Describes the size requirements of a renderable
   ```csharp
   public sealed class Measurement
   {
       public int Min { get; }     // Minimum width needed
       public int Max { get; }     // Maximum width needed
   }
   ```

3. **Segment** - A piece of text with associated style and control sequences
   ```csharp
   public sealed class Segment
   {
       public string Text { get; }
       public Style Style { get; }
       public ControlCode? Control { get; }
   }
   ```

### Key Rendering Classes

#### Text Renderable

- Located in: `src/Spectre.Console/Widgets/Text.cs`
- Handles: Basic text rendering with styles
- Key methods:
  - `Measure()` - Calculates text width using `Cell.GetCellLength()`
  - `Render()` - Splits text into segments based on styles

#### Markup Renderable

- Located in: `src/Spectre.Console/Widgets/Markup.cs`
- Handles: Markup parsing and rendering
- Key components:
  - `MarkupParser` - Parses markup syntax
  - Converts markup to styled text segments

#### TextPath Renderable

- Located in: `src/Spectre.Console/Widgets/TextPath.cs`
- Handles: File path rendering with proper styling
- Features:
  - Path segmentation (folders vs filename)
  - Cross-platform path normalization
  - Proper width calculation

### Critical Width Calculation

The key issue with hyperlinks is that Spectre.Console uses
`Cell.GetCellLength()` to calculate text width, which doesn't account for ANSI
escape sequences.

From `src/Spectre.Console/Internal/Text/Cell.cs`:

```csharp
public static int GetCellLength(ReadOnlySpan<char> text)
{
    // This method calculates visual width
    // It handles Unicode combining characters, emojis, etc.
    // BUT it doesn't handle ANSI escape sequences!
}
```

### Custom Renderable Requirements

To create a hyperlink renderable that works correctly:

1. **Measure() method must:**
   - Calculate width based on display text only (not ANSI sequences)
   - Use `Cell.GetCellLength()` on the visible text
   - Return accurate Measurement

2. **Render() method must:**
   - Generate Segment objects with ANSI sequences embedded
   - Ensure proper text wrapping
   - Handle line breaks correctly

3. **ANSI Integration:**
   - Embed ANSI sequences in Segment.Text
   - Ensure width calculations ignore escape sequences
   - Handle terminal capability detection

### Example Custom Renderable Structure

```csharp
public sealed class HyperlinkText : IRenderable
{
    private readonly string _displayText;
    private readonly string _url;
    private readonly Style _style;

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        // Calculate width based on display text only
        var width = Cell.GetCellLength(_displayText.AsSpan());
        return new Measurement(width, width);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        if (HyperlinkHelper.IsHyperlinkSupported())
        {
            // Create ANSI hyperlink: \e]8;;{url}\e\\{text}\e]8;;\e\\
            var hyperlinkText = $"\u001b]8;;{_url}\u001b\\{_displayText}\u001b]8;;\u001b\\";
            yield return new Segment(hyperlinkText, _style);
        }
        else
        {
            // Fallback to plain text
            yield return new Segment(_displayText, _style);
        }
    }
}
```

### Integration Strategy

1. **Replace TextPath usage** with custom HyperlinkPath
2. **Handle file:// URLs** for local file paths
3. **Maintain visual consistency** with existing displays
4. **Graceful fallback** for non-supporting terminals

### Next Steps

1. Study how TextPath implements path rendering
2. Create HyperlinkPath that extends TextPath functionality
3. Test width calculations with complex layouts
4. Implement in DefaultTreeBuilder and FullPath displays

## Key Insights

- Spectre.Console's strength is in proper width calculation and layout
- ANSI sequences must be embedded in Segment.Text, not added externally
- Width calculation must be based on visible text only
- The rendering pipeline is segment-based, not string-based

## Terminal Environment Differences

### Development vs Production Terminals

One critical finding from this implementation is that **terminal rendering can
vary significantly between development and production environments**:

**Development Terminal Behavior:**

- VS Code integrated terminal may have different ANSI escape sequence handling
- Table layout calculations can be inconsistent with hyperlink markup
- Rendering may vary between runs due to terminal state
- Width calculations may not account for hyperlink markup properly

**Production Terminal Behavior (Windows Terminal):**

- Hyperlinks render correctly and are clickable (Ctrl+Click)
- Table alignment is more consistent
- Color and formatting appear as expected
- ANSI escape sequences are handled more reliably

**Key Recommendations:**

1. **Always test in production terminal** - Don't rely solely on development
   environment rendering
2. **Consider terminal capability detection** - Different terminals have varying
   support for hyperlinks and ANSI sequences
3. **Provide fallback rendering** - For terminals that don't support hyperlinks
   or advanced formatting
4. **Document expected behavior** - Specify which terminals are officially
   supported

### Rendering Consistency Issues

**Observed Variations:**

- Table alignment can differ between runs in development environments
- Hyperlink markup may affect column width calculations inconsistently
- Terminal state (previous output, window size) can influence rendering

**Mitigation Strategies:**

- Use simpler layouts when embedding hyperlinks in tables
- Consider `Rows` of `Markup` instead of complex table structures
- Test with various terminal sizes and states
- Provide configuration options for users with rendering issues

### Example: Robust Rendering for Different Terminals

```csharp
public static void RenderWithFallback(IAnsiConsole console, IRenderable primary, IRenderable fallback)
{
    try
    {
        // Try to detect terminal capabilities
        if (console.Profile.Capabilities.SupportsAnsi &&
            Environment.GetEnvironmentVariable("TERM_PROGRAM") != "vscode")
        {
            console.Write(primary);
        }
        else
        {
            console.Write(fallback);
        }
    }
    catch (Exception)
    {
        // If rendering fails, fall back to simple output
        console.Write(fallback);
    }
}

// Usage example:
var hyperlinkedTree = CreateHyperlinkedTreeView(includeGraph);
var simpleTree = CreateSimpleTreeView(includeGraph);

RenderWithFallback(AnsiConsole.Console, hyperlinkedTree, simpleTree);
```

This approach provides graceful degradation for terminals that don't fully
support advanced rendering features.

## Production Deployment Considerations

When deploying Spectre.Console applications that use advanced rendering
features:

### Terminal Compatibility Testing

1. **Test in target environments**: Don't assume development terminal behavior
   matches production
2. **Document supported terminals**: Windows Terminal, CMD, PowerShell, etc.
3. **Provide configuration options**: Allow users to disable advanced features
   if needed
4. **Consider CI/CD rendering**: Automated builds may have different terminal
   capabilities

### Performance Considerations

- Complex renderables with many hyperlinks can impact performance
- Table calculations become more expensive with markup
- Consider lazy rendering for large data sets
- Cache expensive renderable calculations when possible

### User Experience Guidelines

- Hyperlinks should enhance, not replace, core functionality
- Provide keyboard alternatives to mouse interactions
- Consider accessibility for users with screen readers
- Document terminal-specific behaviors in user guides

## Key Findings from Hyperlink Implementation

### What Works Well

1. **Spectre.Console Markup with Hyperlinks**: Using
   `[link=file:///path]filename[/]` syntax works perfectly in Windows Terminal
2. **Table-based Layout**: Simple table structure with two columns handles
   alignment well
3. **Modular Renderables**: Each column can be its own IRenderable/Markup for
   reuse
4. **URI Generation**: `new Uri(filePath).AbsoluteUri` creates proper file://
   URLs for clicking

### Implementation Pattern

```csharp
// Create individual renderables for each column
var leftColumn = new Markup($"[link={fileUri}]{fileName}[/]");
var rightColumn = new Markup($"[link={fileUri}]{fileName}[/]");

// Add to table as separate cells
table.AddRow(leftColumn, rightColumn);
```

### Key Benefits

- **Reusable Components**: Each column is self-contained
- **Independent Rendering**: Left or right side can be rendered separately
- **Clean Separation**: File structure vs reference tree logic isolated
- **Terminal Compatibility**: Works in Windows Terminal, graceful degradation
  elsewhere

### Original Working Format Characteristics

- Clean table layout with proper alignment
- Side-by-side display without complex panel nesting
- Simple markup-based approach
- Easily configurable individual columns
