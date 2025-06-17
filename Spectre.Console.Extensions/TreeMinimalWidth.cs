using Spectre.Console.Rendering;
using System.Reflection;

namespace Spectre.Console.Extensions;

/// <summary>
/// Extension methods for List operations
/// </summary>
internal static class ListExtensions
{
	public static void RemoveLast<T>(this List<T> list)
	{
		if (list.Count > 0)
			list.RemoveAt(list.Count - 1);
	}

	public static void AddOrReplaceLast<T>(this List<T> list, T item)
	{
		if (list.Count > 0)
			list[^1] = item;
		else
			list.Add(item);
	}
}

/// <summary>
/// Helper class to access TreeNode internals via reflection
/// </summary>
internal static class TreeNodeHelper
{
	private static readonly PropertyInfo? RenderableProperty = typeof(TreeNode).GetProperty("Renderable", BindingFlags.NonPublic | BindingFlags.Instance);

	public static IRenderable GetRenderable(TreeNode node)
	{
		return (IRenderable?)RenderableProperty?.GetValue(node) ?? new Text("");
	}
}

/// <summary>
/// Representation of non-circular tree data.
/// Each node added to the tree may only be present in it a single time, in order to facilitate cycle detection.
/// </summary>
public sealed class TreeMinimalWidth : Renderable, IHasTreeNodes
{
	private readonly TreeNode _root;
	private bool _expanded = true;

	/// <summary>
	/// Gets or sets the tree style.
	/// </summary>
	public Style? Style { get; set; }

	/// <summary>
	///  Gets or sets the tree guide lines.
	/// </summary>
	public TreeGuide Guide { get; set; } = TreeGuide.Line;

	/// <summary>
	/// Gets the tree's child nodes.
	/// </summary>
	public List<TreeNode> Nodes => _root.Nodes;

	/// <summary>
	/// Gets or sets a value indicating whether or not the tree is expanded or not.
	/// </summary>
	public bool Expanded
	{
		get => _expanded;
		set
		{
			_expanded = value;
			_root.Expand(value);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeMinimalWidth"/> class.
	/// </summary>
	/// <param name="renderable">The tree label.</param>
	public TreeMinimalWidth(IRenderable renderable)
	{
		_root = new TreeNode(renderable);
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="TreeMinimalWidth"/> class.
	/// </summary>
	/// <param name="label">The tree label.</param>
	public TreeMinimalWidth(string label)
	{
		_root = new TreeNode(new Markup(label));
	}
	/// <inheritdoc />
	protected override Measurement Measure(RenderOptions options, int maxWidth)
	{
		// Calculate the actual content width needed
		var contentWidth = CalculateMinimalWidth(options, maxWidth);

		// Return proper min/max: 
		// - Min: reasonable minimum to avoid excessive wrapping (but allow some compression)
		// - Max: the natural content width for best display
		var minWidth = Math.Min(contentWidth, Math.Max(20, contentWidth * 2 / 3)); // At least 20, or 2/3 of natural width
		return new Measurement(minWidth, contentWidth);
	}

	/// <inheritdoc />
	protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
	{
		// TWO-PASS APPROACH:
		// Pass 1: Calculate minimal width needed for content
		// Pass 2: Render with that minimal width

		var minimalWidth = CalculateMinimalWidth(options, maxWidth);
		return RenderWithWidth(options, minimalWidth);
	}

	/// <summary>
	/// Calculate the minimal width needed to render all tree content
	/// </summary>
	private int CalculateMinimalWidth(RenderOptions options, int maxWidth)
	{
		var visitedNodes = new HashSet<TreeNode>();
		var stack = new Stack<Queue<TreeNode>>();
		stack.Push(new Queue<TreeNode>([_root]));

		var levels = new List<Segment>
		{
			GetGuide(options, TreeGuidePart.Continue)
		};

		var maxRequiredWidth = 0;

		while (stack.Count > 0)
		{
			var stackNode = stack.Pop();
			if (stackNode.Count == 0)
			{
				levels.RemoveLast();
				if (levels.Count > 0)
					levels.AddOrReplaceLast(GetGuide(options, TreeGuidePart.Fork));
				continue;
			}

			var isLastChild = stackNode.Count == 1;
			var current = stackNode.Dequeue();
			if (!visitedNodes.Add(current))
				continue; // Skip if already visited to avoid cycles

			stack.Push(stackNode);

			if (isLastChild)
				levels.AddOrReplaceLast(GetGuide(options, TreeGuidePart.End)); var prefix = levels.Skip(1).ToList();
			var prefixWidth = Segment.CellCount(prefix);

			// Get the renderable and calculate its natural width
			var renderable = TreeNodeHelper.GetRenderable(current);

			// For proper measurement: give it generous space to measure its natural size
			var measurement = renderable.Measure(options, Math.Max(maxWidth - prefixWidth, 50));

			// Use the Max (preferred) width rather than Min to avoid unnecessary wrapping
			// This ensures icons and text display properly without being squished
			var contentWidth = measurement.Max;
			var totalRequiredWidth = prefixWidth + contentWidth;

			maxRequiredWidth = Math.Max(maxRequiredWidth, totalRequiredWidth);

			if (current.Expanded && current.Nodes.Count > 0)
			{
				levels.AddOrReplaceLast(GetGuide(options, isLastChild ? TreeGuidePart.Space : TreeGuidePart.Continue));
				levels.Add(GetGuide(options, current.Nodes.Count == 1 ? TreeGuidePart.End : TreeGuidePart.Fork));

				stack.Push(new Queue<TreeNode>(current.Nodes));
			}
		}

		return Math.Min(maxRequiredWidth, maxWidth);
	}

	/// <summary>
	/// Render the tree with the specified width
	/// </summary>
	private List<Segment> RenderWithWidth(RenderOptions options, int renderWidth)
	{
		var result = new List<Segment>();
		var visitedNodes = new HashSet<TreeNode>();

		var stack = new Stack<Queue<TreeNode>>();
		stack.Push(new Queue<TreeNode>([_root]));

		var levels = new List<Segment>
		{
			GetGuide(options, TreeGuidePart.Continue)
		};

		while (stack.Count > 0)
		{
			var stackNode = stack.Pop();
			if (stackNode.Count == 0)
			{
				levels.RemoveLast();
				if (levels.Count > 0)
					levels.AddOrReplaceLast(GetGuide(options, TreeGuidePart.Fork));

				continue;
			}

			var isLastChild = stackNode.Count == 1;
			var current = stackNode.Dequeue(); if (!visitedNodes.Add(current))
				throw new InvalidOperationException("Cycle detected in tree - unable to render.");

			stack.Push(stackNode);

			if (isLastChild)
				levels.AddOrReplaceLast(GetGuide(options, TreeGuidePart.End)); var prefix = levels.Skip(1).ToList();
			// Use our calculated minimal width instead of maxWidth
			var renderable = TreeNodeHelper.GetRenderable(current);
			var renderableLines = Segment.SplitLines(renderable.Render(options, renderWidth - Segment.CellCount(prefix)));

			for (int i = 0; i < renderableLines.Count; i++)
			{
				var isFirstLine = i == 0;
				var line = renderableLines[i];

				if (prefix.Count > 0)
					result.AddRange(prefix.ToList());

				result.AddRange(line);
				result.Add(Segment.LineBreak);

				if (isFirstLine && prefix.Count > 0)
				{
					var part = isLastChild ? TreeGuidePart.Space : TreeGuidePart.Continue;
					prefix.AddOrReplaceLast(GetGuide(options, part));
				}
			}

			if (current.Expanded && current.Nodes.Count > 0)
			{
				levels.AddOrReplaceLast(GetGuide(options, isLastChild ? TreeGuidePart.Space : TreeGuidePart.Continue));
				levels.Add(GetGuide(options, current.Nodes.Count == 1 ? TreeGuidePart.End : TreeGuidePart.Fork));

				stack.Push(new Queue<TreeNode>(current.Nodes));
			}
		}

		return result;
	}

	private Segment GetGuide(RenderOptions options, TreeGuidePart part)
	{
		var guide = Guide.GetSafeTreeGuide(safe: !options.Unicode);
		return new Segment(guide.GetPart(part), Style ?? Style.Plain);
	}
}

/// <summary>
/// Extension methods for IEnumerable operations
/// </summary>
internal static class EnumerableExtensions
{
	public static IEnumerable<(int Index, bool IsFirst, bool IsLast, T Item)> Enumerate<T>(this IEnumerable<T> source)
	{
		var list = source.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			yield return (i, i == 0, i == list.Count - 1, list[i]);
		}
	}
}