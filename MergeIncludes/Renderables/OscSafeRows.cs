using Spectre.Console.Rendering;
using System.Collections.Immutable;

namespace MergeIncludes.Renderables;
public sealed class OscSafeRows(params ImmutableArray<IRenderable> children) : IRenderable
{
	public OscSafeRows(IEnumerable<IRenderable> children) : this(children is ImmutableArray<IRenderable> c ? c : children.ToImmutableArray())
	{
	}

	public Measurement Measure(RenderOptions options, int maxWidth)
	{
		int min = 0, max = 0;
		foreach (var child in children)
		{
			var m = child.Measure(options, maxWidth);
			min = Math.Max(min, m.Min);
			max = Math.Max(max, m.Max);
		}

		return new Measurement(min, max);
	}

	public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
	{
		if (children.Length == 0)
			yield break;

		foreach (var item in children[0].Render(options, maxWidth))
		{
			yield return item;
		}

		if (children.Length == 1)
			yield break;

		for (var i = 1; i < children.Length; i++)
		{
			yield return Segment.LineBreak;
			foreach (var item in children[i].Render(options, maxWidth))
			{
				yield return item;
			}
		}
	}
}
