using Spectre.Console.Rendering;

namespace Spectre.Console.Extensions;

public class PanelBuilder : IRenderable
{
	readonly List<IRenderable> _contents;
	readonly Lazy<IRenderable> _panel;

	public PanelBuilder(string headerText, bool expand = true)
	{
		_contents = [];
		_panel = new Lazy<IRenderable>(
			() => new Panel(new Rows(_contents))
			{
				Header = new PanelHeader(headerText),
				Expand = expand
			});
	}

	public void Add(IRenderable renderable)
	{
		if (_panel.IsValueCreated) throw new InvalidOperationException("Panel was already consumed.");
		_contents.Add(renderable);
	}

	public void Add(IEnumerable<IRenderable> renderable)
	{
		if (_panel.IsValueCreated) throw new InvalidOperationException("Panel was already consumed.");
		_contents.AddRange(renderable);
	}
	public Measurement Measure(RenderOptions options, int maxWidth)
		=> _panel.Value.Measure(options, maxWidth);

	public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
		=> _panel.Value.Render(options, maxWidth);
}
