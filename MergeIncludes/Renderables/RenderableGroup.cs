using Spectre.Console.Rendering;
using System.Collections.Immutable;

namespace MergeIncludes.Renderables;

public class RenderableGroup(ImmutableArray<IRenderable> renderables) : RenderableBase
{
	public override Measurement Measure(RenderOptions options, int maxWidth)
		=> renderables.Aggregate(new Measurement(0, 0), (acc, r) =>
		{
			var measurement = r.Measure(options, maxWidth);
			return new Measurement(Math.Max(acc.Min, measurement.Min), Math.Max(acc.Max, measurement.Max));
		});

	public override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
		=> renderables.SelectMany(r => r.Render(options, maxWidth));
}