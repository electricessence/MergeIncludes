using Spectre.Console.Rendering;

namespace MergeIncludes.Renderables;

/// <summary>
/// Base class for custom renderables that provides common functionality
/// </summary>
public abstract class RenderableBase : IRenderable
{
	public abstract Measurement Measure(RenderOptions options, int maxWidth);

	public abstract IEnumerable<Segment> Render(RenderOptions options, int maxWidth);
}
